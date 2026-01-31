extends Node

## NetworkManager Singleton
## Handles all multiplayer networking for Fish Catcher VS mode

# Signals for game events
signal connected_to_server
signal connection_failed
signal server_disconnected
signal player_connected(peer_id: int, player_info: Dictionary)
signal player_disconnected(peer_id: int)
signal game_started(players: Dictionary)
signal opponent_score_updated(score: int)
signal opponent_caught_fish(fish_id: int)
signal fish_spawn_received(fish_data: Array)
signal game_over_received(winner_id: int, scores: Dictionary)
signal opponent_claw_updated(position: Vector2, head_y: float, state: int, has_fish: bool)

# Network constants
const DEFAULT_PORT: int = 7777
const MAX_PLAYERS: int = 2

# Player data
var player_info: Dictionary = {
	"name": "Player",
	"score": 0,
	"ready": false
}

# Connected players: peer_id -> player_info
var players: Dictionary = {}

# Network state
var peer: ENetMultiplayerPeer = null
var is_host: bool = false
var is_connected: bool = false
var my_id: int = 0

func _ready() -> void:
	# Connect multiplayer signals
	multiplayer.peer_connected.connect(_on_peer_connected)
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)
	multiplayer.connected_to_server.connect(_on_connected_to_server)
	multiplayer.connection_failed.connect(_on_connection_failed)
	multiplayer.server_disconnected.connect(_on_server_disconnected)

## Host a new game server
func host_game(player_name: String = "Host") -> Error:
	player_info.name = player_name
	player_info.score = 0

	peer = ENetMultiplayerPeer.new()
	var error = peer.create_server(DEFAULT_PORT, MAX_PLAYERS)

	if error != OK:
		print("Failed to create server: ", error)
		return error

	multiplayer.multiplayer_peer = peer
	is_host = true
	is_connected = true
	my_id = 1  # Host is always peer ID 1

	# Add self to players list
	players[1] = player_info.duplicate()

	print("Server started on port ", DEFAULT_PORT)
	return OK

## Join an existing game
func join_game(address: String, player_name: String = "Guest") -> Error:
	player_info.name = player_name
	player_info.score = 0

	peer = ENetMultiplayerPeer.new()
	var error = peer.create_client(address, DEFAULT_PORT)

	if error != OK:
		print("Failed to connect to server: ", error)
		return error

	multiplayer.multiplayer_peer = peer
	is_host = false

	print("Connecting to ", address, ":", DEFAULT_PORT)
	return OK

## Disconnect from current game
func disconnect_game() -> void:
	if peer:
		peer.close()

	multiplayer.multiplayer_peer = null
	peer = null
	players.clear()
	is_host = false
	is_connected = false
	my_id = 0
	player_info.score = 0
	player_info.ready = false

	print("Disconnected from game")

## Called when any peer connects
func _on_peer_connected(id: int) -> void:
	print("Peer connected: ", id)
	# Send our player info to the new peer
	rpc_id(id, "_register_player", player_info)

## Called when any peer disconnects
func _on_peer_disconnected(id: int) -> void:
	print("Peer disconnected: ", id)
	players.erase(id)
	player_disconnected.emit(id)

## Called on client when connected to server
func _on_connected_to_server() -> void:
	print("Connected to server!")
	is_connected = true
	my_id = multiplayer.get_unique_id()
	connected_to_server.emit()

## Called on client when connection fails
func _on_connection_failed() -> void:
	print("Connection failed!")
	is_connected = false
	multiplayer.multiplayer_peer = null
	peer = null
	connection_failed.emit()

## Called on client when server disconnects
func _on_server_disconnected() -> void:
	print("Server disconnected!")
	is_connected = false
	multiplayer.multiplayer_peer = null
	peer = null
	players.clear()
	server_disconnected.emit()

## Register a player (called via RPC)
@rpc("any_peer", "reliable")
func _register_player(info: Dictionary) -> void:
	var sender_id = multiplayer.get_remote_sender_id()
	players[sender_id] = info
	print("Player registered: ", info.name, " (ID: ", sender_id, ")")

	player_connected.emit(sender_id, info)

	# If host and we have 2 players, we can start
	if is_host and players.size() >= 2:
		print("Lobby full! Ready to start game.")

## Host starts the game
func start_multiplayer_game() -> void:
	if not is_host:
		print("Only host can start the game")
		return

	if players.size() < 2:
		print("Need 2 players to start")
		return

	# Generate fish spawn data
	var fish_data = _generate_fish_spawn_data()

	# Tell all clients to start
	rpc("_receive_game_start", players, fish_data)

@rpc("authority", "reliable", "call_local")
func _receive_game_start(all_players: Dictionary, fish_data: Array) -> void:
	players = all_players
	fish_spawn_received.emit(fish_data)
	game_started.emit(players)

## Generate synchronized fish spawn data (host only)
func _generate_fish_spawn_data() -> Array:
	var fish_data: Array = []
	var fish_colors = ["red", "orange", "yellow", "green", "blue", "purple"]

	for i in range(15):
		var row = i / 5
		var col = i % 5

		fish_data.append({
			"id": i,
			"x": 60 + col * 140 + randf_range(-20, 20),
			"y": row * 80 + randf_range(-10, 10),
			"color": fish_colors[i % fish_colors.size()],
			"speed": randf_range(50.0, 120.0),
			"direction": 1 if randf() > 0.5 else -1
		})

	return fish_data

## Report that local player caught a fish
func report_fish_caught(fish_id: int) -> void:
	player_info.score += 1
	rpc("_sync_fish_caught", my_id, player_info.score, fish_id)

@rpc("any_peer", "reliable", "call_local")
func _sync_fish_caught(peer_id: int, new_score: int, fish_id: int) -> void:
	if peer_id in players:
		players[peer_id].score = new_score

	# If it's not us, emit opponent signal
	if peer_id != my_id:
		opponent_score_updated.emit(new_score)
		opponent_caught_fish.emit(fish_id)

## Host reports game over
func report_game_over() -> void:
	if not is_host:
		return

	var scores = {}
	var winner_id = 0
	var max_score = -1

	for pid in players:
		scores[pid] = players[pid].score
		if players[pid].score > max_score:
			max_score = players[pid].score
			winner_id = pid

	rpc("_receive_game_over", winner_id, scores)

@rpc("authority", "reliable", "call_local")
func _receive_game_over(winner_id: int, scores: Dictionary) -> void:
	game_over_received.emit(winner_id, scores)

## Get opponent info
func get_opponent_info() -> Dictionary:
	for pid in players:
		if pid != my_id:
			return players[pid]
	return {}

## Get my score
func get_my_score() -> int:
	return player_info.score

## Get opponent score
func get_opponent_score() -> int:
	var opponent = get_opponent_info()
	return opponent.get("score", 0)

## Reset scores for new game
func reset_scores() -> void:
	player_info.score = 0
	for pid in players:
		players[pid].score = 0

## Sync claw position to other players (called frequently)
func sync_claw_position(pos: Vector2, head_y: float, state: int, has_fish: bool) -> void:
	if not is_connected:
		return
	# Use unreliable for frequent position updates (better performance)
	rpc("_receive_claw_position", pos, head_y, state, has_fish)

@rpc("any_peer", "unreliable_ordered")
func _receive_claw_position(pos: Vector2, head_y: float, state: int, has_fish: bool) -> void:
	var sender_id = multiplayer.get_remote_sender_id()
	# Only process if it's from opponent
	if sender_id != my_id:
		opponent_claw_updated.emit(pos, head_y, state, has_fish)
