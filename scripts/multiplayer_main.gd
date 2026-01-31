extends Node2D

## Multiplayer VS Mode Game Controller

# Game settings
const GAME_TIME: int = 30

# Game state
var time_remaining: int = GAME_TIME
var game_active: bool = false
var synced_fish_data: Array = []

# Node references
@onready var local_score_label: Label = $UI/TopBar/LocalScore
@onready var opponent_score_label: Label = $UI/TopBar/OpponentScore
@onready var timer_label: Label = $UI/TopBar/Timer
@onready var vs_label: Label = $UI/VSLabel
@onready var game_timer: Timer = $GameTimer
@onready var fish_spawner: Node2D = $FishSpawner
@onready var claw: Node2D = $Claw
@onready var opponent_claw: Node2D = $OpponentClaw
@onready var drop_zone: Area2D = $DropZone
@onready var countdown_panel: Panel = $UI/CountdownPanel
@onready var countdown_label: Label = $UI/CountdownPanel/CountdownLabel
@onready var game_over_panel: Panel = $UI/GameOverPanel
@onready var result_label: Label = $UI/GameOverPanel/ResultLabel
@onready var scores_label: Label = $UI/GameOverPanel/ScoresLabel
@onready var rematch_button: Button = $UI/GameOverPanel/RematchButton
@onready var quit_button: Button = $UI/GameOverPanel/QuitButton
@onready var local_name_label: Label = $UI/TopBar/LocalName
@onready var opponent_name_label: Label = $UI/TopBar/OpponentName

# Claw sync timing
const CLAW_SYNC_INTERVAL: float = 0.05  # 20 updates per second
var claw_sync_timer: float = 0.0

func _ready() -> void:
	# Connect signals
	game_timer.timeout.connect(_on_game_timer_timeout)
	drop_zone.fish_dropped.connect(_on_fish_dropped)
	rematch_button.pressed.connect(_on_rematch_pressed)
	quit_button.pressed.connect(_on_quit_pressed)

	# Connect network signals
	NetworkManager.opponent_score_updated.connect(_on_opponent_score_updated)
	NetworkManager.opponent_caught_fish.connect(_on_opponent_caught_fish)
	NetworkManager.fish_spawn_received.connect(_on_fish_spawn_received)
	NetworkManager.game_over_received.connect(_on_game_over_received)
	NetworkManager.server_disconnected.connect(_on_server_disconnected)
	NetworkManager.player_disconnected.connect(_on_player_disconnected)
	NetworkManager.opponent_claw_updated.connect(_on_opponent_claw_updated)

	# Initial UI state
	countdown_panel.visible = true
	game_over_panel.visible = false
	vs_label.visible = true

	# Disable gameplay until countdown finishes
	claw.set_process(false)
	claw.set_process_input(false)
	fish_spawner.set_process(false)

	# Setup player names
	_setup_player_names()

	# Start countdown
	_start_countdown()

func _setup_player_names() -> void:
	local_name_label.text = NetworkManager.player_info.name
	var opponent = NetworkManager.get_opponent_info()
	var opponent_name = opponent.get("name", "Opponent")
	opponent_name_label.text = opponent_name

	# Set opponent claw name label
	if opponent_claw:
		opponent_claw.set_opponent_name(opponent_name)

func _process(delta: float) -> void:
	if not game_active:
		return

	# Sync local claw position to opponent
	claw_sync_timer += delta
	if claw_sync_timer >= CLAW_SYNC_INTERVAL:
		claw_sync_timer = 0.0
		_sync_local_claw()

func _sync_local_claw() -> void:
	if not claw:
		return

	var pos = claw.position
	var head_y = claw.claw_head.position.y
	# Send is_touching as state indicator (1 = descending, 0 = ascending)
	var state = 1 if claw.is_touching else 0
	var has_fish = claw.has_fish()

	NetworkManager.sync_claw_position(pos, head_y, state, has_fish)

func _on_opponent_claw_updated(pos: Vector2, head_y: float, state: int, has_fish: bool) -> void:
	if opponent_claw:
		opponent_claw.update_from_network(pos, head_y, state, has_fish)

func _start_countdown() -> void:
	# 3-2-1 countdown before game starts
	countdown_label.text = "3"
	await get_tree().create_timer(1.0).timeout
	countdown_label.text = "2"
	await get_tree().create_timer(1.0).timeout
	countdown_label.text = "1"
	await get_tree().create_timer(1.0).timeout
	countdown_label.text = "GO!"
	await get_tree().create_timer(0.5).timeout

	countdown_panel.visible = false
	_start_game()

func _start_game() -> void:
	NetworkManager.reset_scores()
	time_remaining = GAME_TIME
	game_active = true
	_update_ui()

	# Spawn fish from synced data if available
	if synced_fish_data.size() > 0:
		fish_spawner.spawn_from_network_data(synced_fish_data)
	else:
		fish_spawner.reset_fish()

	# Enable gameplay
	claw.set_process(true)
	claw.set_process_input(true)
	claw.reset()
	fish_spawner.set_process(true)

	game_timer.start()

func _on_game_timer_timeout() -> void:
	if game_active:
		time_remaining -= 1
		_update_ui()
		if time_remaining <= 0:
			_end_game()

func _on_fish_dropped() -> void:
	if game_active:
		# Get the fish ID that was caught (if tracked)
		var fish_id = claw.get_grabbed_fish_id()
		NetworkManager.report_fish_caught(fish_id)
		_update_ui()

func _on_opponent_score_updated(score: int) -> void:
	_update_ui()

func _on_opponent_caught_fish(fish_id: int) -> void:
	# Visual feedback that opponent caught a fish
	# Could flash the opponent's score or show indicator
	_flash_opponent_score()

func _on_fish_spawn_received(fish_data: Array) -> void:
	synced_fish_data = fish_data

func _on_game_over_received(winner_id: int, scores: Dictionary) -> void:
	_show_game_over(winner_id, scores)

func _on_server_disconnected() -> void:
	_show_disconnect_message("Host disconnected!")

func _on_player_disconnected(_peer_id: int) -> void:
	_show_disconnect_message("Opponent disconnected!")

func _update_ui() -> void:
	local_score_label.text = str(NetworkManager.get_my_score())
	opponent_score_label.text = str(NetworkManager.get_opponent_score())
	timer_label.text = str(time_remaining)

func _flash_opponent_score() -> void:
	var tween = create_tween()
	tween.tween_property(opponent_score_label, "modulate", Color(1, 1, 0), 0.1)
	tween.tween_property(opponent_score_label, "modulate", Color(1, 1, 1), 0.1)

func _end_game() -> void:
	game_active = false
	game_timer.stop()

	# Disable gameplay
	claw.set_process(false)
	claw.set_process_input(false)
	fish_spawner.set_process(false)

	# Host reports game over to all players
	if NetworkManager.is_host:
		NetworkManager.report_game_over()

func _show_game_over(winner_id: int, scores: Dictionary) -> void:
	game_over_panel.visible = true

	var my_id = NetworkManager.my_id
	var my_score = scores.get(my_id, 0)
	var opponent_score = 0
	var opponent_id = 0

	for pid in scores:
		if pid != my_id:
			opponent_score = scores[pid]
			opponent_id = pid
			break

	# Determine result
	if winner_id == my_id:
		result_label.text = "YOU WIN!"
		result_label.modulate = Color(0.2, 1.0, 0.4)
	elif winner_id == opponent_id:
		result_label.text = "YOU LOSE"
		result_label.modulate = Color(1.0, 0.4, 0.4)
	else:
		result_label.text = "TIE!"
		result_label.modulate = Color(1.0, 1.0, 0.4)

	scores_label.text = "You: %d  -  Opponent: %d" % [my_score, opponent_score]

	# Only host can rematch
	rematch_button.visible = NetworkManager.is_host

func _show_disconnect_message(message: String) -> void:
	game_active = false
	game_timer.stop()

	game_over_panel.visible = true
	result_label.text = message
	result_label.modulate = Color(1.0, 0.6, 0.2)
	scores_label.text = "Game ended"
	rematch_button.visible = false

func _on_rematch_pressed() -> void:
	if NetworkManager.is_host:
		# Reset and restart
		game_over_panel.visible = false
		countdown_panel.visible = true
		NetworkManager.start_multiplayer_game()

func _on_quit_pressed() -> void:
	NetworkManager.disconnect_game()
	get_tree().change_scene_to_file("res://scenes/main.tscn")
