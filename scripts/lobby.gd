extends Control

## Lobby UI for multiplayer matchmaking

# UI References
@onready var name_input: LineEdit = $VBoxContainer/NameInput
@onready var host_button: Button = $VBoxContainer/HostButton
@onready var join_container: VBoxContainer = $VBoxContainer/JoinContainer
@onready var ip_input: LineEdit = $VBoxContainer/JoinContainer/IPInput
@onready var join_button: Button = $VBoxContainer/JoinContainer/JoinButton
@onready var status_label: Label = $VBoxContainer/StatusLabel
@onready var waiting_panel: Panel = $WaitingPanel
@onready var waiting_label: Label = $WaitingPanel/WaitingLabel
@onready var cancel_button: Button = $WaitingPanel/CancelButton
@onready var start_button: Button = $WaitingPanel/StartButton
@onready var back_button: Button = $VBoxContainer/BackButton

func _ready() -> void:
	# Connect UI signals
	host_button.pressed.connect(_on_host_pressed)
	join_button.pressed.connect(_on_join_pressed)
	cancel_button.pressed.connect(_on_cancel_pressed)
	start_button.pressed.connect(_on_start_pressed)
	back_button.pressed.connect(_on_back_pressed)

	# Connect network signals
	NetworkManager.connected_to_server.connect(_on_connected_to_server)
	NetworkManager.connection_failed.connect(_on_connection_failed)
	NetworkManager.player_connected.connect(_on_player_connected)
	NetworkManager.player_disconnected.connect(_on_player_disconnected)
	NetworkManager.game_started.connect(_on_game_started)
	NetworkManager.server_disconnected.connect(_on_server_disconnected)

	# Initial UI state
	waiting_panel.visible = false
	start_button.visible = false
	status_label.text = "Enter your name and host or join a game"

func _on_host_pressed() -> void:
	var player_name = name_input.text.strip_edges()
	if player_name.is_empty():
		player_name = "Host"

	var error = NetworkManager.host_game(player_name)

	if error == OK:
		_show_waiting_panel("Hosting game...\nWaiting for opponent to join\n\nYour IP: " + _get_local_ip())
		start_button.visible = false
	else:
		status_label.text = "Failed to host game (Error: %d)" % error

func _on_join_pressed() -> void:
	var player_name = name_input.text.strip_edges()
	if player_name.is_empty():
		player_name = "Guest"

	var address = ip_input.text.strip_edges()
	if address.is_empty():
		address = "127.0.0.1"

	var error = NetworkManager.join_game(address, player_name)

	if error == OK:
		_show_waiting_panel("Connecting to " + address + "...")
	else:
		status_label.text = "Failed to connect (Error: %d)" % error

func _on_cancel_pressed() -> void:
	NetworkManager.disconnect_game()
	_hide_waiting_panel()
	status_label.text = "Disconnected"

func _on_start_pressed() -> void:
	if NetworkManager.is_host:
		NetworkManager.start_multiplayer_game()

func _on_back_pressed() -> void:
	NetworkManager.disconnect_game()
	get_tree().change_scene_to_file("res://scenes/main.tscn")

func _on_connected_to_server() -> void:
	waiting_label.text = "Connected!\nWaiting for host to start..."

func _on_connection_failed() -> void:
	_hide_waiting_panel()
	status_label.text = "Connection failed! Check the IP address."

func _on_server_disconnected() -> void:
	_hide_waiting_panel()
	status_label.text = "Host disconnected!"

func _on_player_connected(peer_id: int, player_info: Dictionary) -> void:
	waiting_label.text = "Opponent joined: " + player_info.name + "\n\nReady to play!"

	# Show start button for host
	if NetworkManager.is_host:
		start_button.visible = true

func _on_player_disconnected(_peer_id: int) -> void:
	waiting_label.text = "Opponent disconnected!\nWaiting for new opponent..."
	start_button.visible = false

func _on_game_started(_players: Dictionary) -> void:
	# Switch to multiplayer game scene
	get_tree().change_scene_to_file("res://scenes/multiplayer_main.tscn")

func _show_waiting_panel(text: String) -> void:
	waiting_panel.visible = true
	waiting_label.text = text
	_disable_inputs()

func _hide_waiting_panel() -> void:
	waiting_panel.visible = false
	start_button.visible = false
	_enable_inputs()

func _disable_inputs() -> void:
	name_input.editable = false
	host_button.disabled = true
	join_button.disabled = true
	ip_input.editable = false

func _enable_inputs() -> void:
	name_input.editable = true
	host_button.disabled = false
	join_button.disabled = false
	ip_input.editable = true

func _get_local_ip() -> String:
	var addresses = IP.get_local_addresses()
	for addr in addresses:
		# Return first IPv4 non-localhost address
		if addr.begins_with("192.") or addr.begins_with("10.") or addr.begins_with("172."):
			return addr
	return "127.0.0.1"
