extends Node2D

# Game settings
const TARGET_FISH: int = 10
const GAME_TIME: int = 30

# Game state
var score: int = 0
var time_remaining: int = GAME_TIME
var game_active: bool = false
var is_paused: bool = false

# Node references
@onready var score_label: Label = $UI/ScoreLabel
@onready var timer_label: Label = $UI/TimerLabel
@onready var timer_button: Button = $UI/TimerButton
@onready var game_timer: Timer = $GameTimer
@onready var start_panel: Panel = $UI/StartPanel
@onready var start_button: Button = $UI/StartPanel/StartButton
@onready var game_over_panel: Panel = $UI/GameOverPanel
@onready var result_label: Label = $UI/GameOverPanel/ResultLabel
@onready var win_lose_label: Label = $UI/GameOverPanel/WinLoseLabel
@onready var restart_button: Button = $UI/GameOverPanel/RestartButton
@onready var home_button_gameover: Button = $UI/GameOverPanel/HomeButton
@onready var pause_panel: Panel = $UI/PausePanel
@onready var resume_button: Button = $UI/PausePanel/ResumeButton
@onready var restart_pause_button: Button = $UI/PausePanel/RestartButton
@onready var home_button_pause: Button = $UI/PausePanel/HomeButton
@onready var fish_spawner: Node2D = $FishSpawner
@onready var claw: Node2D = $Claw
@onready var drop_zone: Area2D = $DropZone

func _ready() -> void:
	game_timer.timeout.connect(_on_game_timer_timeout)
	start_button.pressed.connect(_on_start_button_pressed)
	restart_button.pressed.connect(_on_restart_button_pressed)
	drop_zone.fish_dropped.connect(_on_fish_dropped)

	# Pause menu buttons
	if timer_button:
		timer_button.pressed.connect(_on_timer_pressed)
	if resume_button:
		resume_button.pressed.connect(_on_resume_pressed)
	if restart_pause_button:
		restart_pause_button.pressed.connect(_on_restart_from_pause)
	if home_button_pause:
		home_button_pause.pressed.connect(_on_home_pressed)
	if home_button_gameover:
		home_button_gameover.pressed.connect(_on_home_pressed)

	# Hide panels
	pause_panel.visible = false
	game_over_panel.visible = false
	start_panel.visible = false

	# Start game immediately (home screen handles menu)
	start_game()

func _on_start_button_pressed() -> void:
	start_panel.visible = false
	start_game()

func _on_restart_button_pressed() -> void:
	game_over_panel.visible = false
	start_game()

func start_game() -> void:
	score = 0
	time_remaining = GAME_TIME
	game_active = true
	is_paused = false
	update_ui()

	# Enable gameplay
	claw.set_process(true)
	claw.set_process_input(true)
	claw.reset()
	fish_spawner.set_process(true)
	fish_spawner.reset_fish()

	game_timer.start()

func _on_game_timer_timeout() -> void:
	if game_active and not is_paused:
		time_remaining -= 1
		update_ui()
		if time_remaining <= 0:
			end_game()

func _on_fish_dropped() -> void:
	if game_active and not is_paused:
		score += 1
		update_ui()

		# Check for win
		if score >= TARGET_FISH:
			end_game()

func update_ui() -> void:
	score_label.text = "Fish: " + str(score) + " / " + str(TARGET_FISH)
	timer_label.text = "Time: " + str(time_remaining)

func end_game() -> void:
	game_active = false
	game_timer.stop()

	# Disable gameplay
	claw.set_process(false)
	claw.set_process_input(false)
	fish_spawner.set_process(false)

	# Show results
	result_label.text = "You caught " + str(score) + " fish!"
	if score >= TARGET_FISH:
		win_lose_label.text = "YOU WIN!"
		win_lose_label.modulate = Color(0.2, 1.0, 0.4)
	else:
		win_lose_label.text = "TRY AGAIN!"
		win_lose_label.modulate = Color(1.0, 0.4, 0.4)

	game_over_panel.visible = true

# Pause functionality
func _on_timer_pressed() -> void:
	if game_active and not is_paused:
		pause_game()

func pause_game() -> void:
	is_paused = true
	game_timer.paused = true

	# Disable gameplay
	claw.set_process(false)
	claw.set_process_input(false)
	fish_spawner.set_process(false)

	pause_panel.visible = true

func _on_resume_pressed() -> void:
	resume_game()

func resume_game() -> void:
	is_paused = false
	game_timer.paused = false

	# Enable gameplay
	claw.set_process(true)
	claw.set_process_input(true)
	fish_spawner.set_process(true)

	pause_panel.visible = false

func _on_restart_from_pause() -> void:
	pause_panel.visible = false
	game_timer.paused = false
	start_game()

func _on_home_pressed() -> void:
	game_timer.stop()
	game_timer.paused = false
	get_tree().change_scene_to_file("res://scenes/home_screen.tscn")
