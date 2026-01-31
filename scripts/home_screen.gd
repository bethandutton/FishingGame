extends Control

## Home Screen - Main Menu

@onready var single_player_button: Button = $VBoxContainer/SinglePlayerButton
@onready var multiplayer_button: Button = $VBoxContainer/MultiplayerButton
@onready var title_label: Label = $TitleLabel
@onready var fish_container: Node2D = $FishContainer

# Decorative fish animation
var fish_positions: Array = []
var fish_speeds: Array = []

func _ready() -> void:
	single_player_button.pressed.connect(_on_single_player_pressed)
	multiplayer_button.pressed.connect(_on_multiplayer_pressed)

	# Spawn decorative swimming fish in background
	_spawn_decorative_fish()

	# Animate title
	_animate_title()

func _on_single_player_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/main.tscn")

func _on_multiplayer_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/lobby.tscn")

func _spawn_decorative_fish() -> void:
	var fish_colors = [
		Color(1.0, 0.3, 0.3),   # Red
		Color(1.0, 0.6, 0.2),   # Orange
		Color(1.0, 0.9, 0.2),   # Yellow
		Color(0.3, 0.8, 0.3),   # Green
		Color(0.3, 0.5, 1.0),   # Blue
		Color(0.6, 0.3, 0.8),   # Purple
	]

	for i in range(8):
		var fish = _create_simple_fish(fish_colors[i % fish_colors.size()])
		fish.position = Vector2(randf_range(100, 620), randf_range(700, 1100))
		fish.scale = Vector2(0.8, 0.8)
		fish_container.add_child(fish)

		fish_positions.append(fish.position.x)
		fish_speeds.append(randf_range(30, 80) * (1 if randf() > 0.5 else -1))

func _create_simple_fish(color: Color) -> Node2D:
	var fish = Node2D.new()

	# Body
	var body = Polygon2D.new()
	body.color = color
	var points = PackedVector2Array()
	for j in range(12):
		var angle = (2 * PI * j) / 12
		var rx = 28.0 * (0.9 if j >= 9 or j <= 3 else 1.0)
		var ry = 25.0
		points.append(Vector2(cos(angle) * rx, sin(angle) * ry))
	body.polygon = points
	fish.add_child(body)

	# Eye
	var eye = Polygon2D.new()
	eye.color = Color.WHITE
	eye.polygon = _circle_points(8, 10)
	eye.position = Vector2(5, -8)
	fish.add_child(eye)

	var pupil = Polygon2D.new()
	pupil.color = Color.BLACK
	pupil.polygon = _circle_points(4, 8)
	pupil.position = Vector2(7, -8)
	fish.add_child(pupil)

	return fish

func _circle_points(radius: float, segments: int) -> PackedVector2Array:
	var points = PackedVector2Array()
	for i in range(segments):
		var angle = (2 * PI * i) / segments
		points.append(Vector2(cos(angle), sin(angle)) * radius)
	return points

func _process(delta: float) -> void:
	# Animate decorative fish
	var children = fish_container.get_children()
	for i in range(children.size()):
		var fish = children[i]
		fish.position.x += fish_speeds[i] * delta

		# Reverse at edges
		if fish.position.x > 700:
			fish_speeds[i] = -abs(fish_speeds[i])
			fish.scale.x = -0.8
		elif fish.position.x < 20:
			fish_speeds[i] = abs(fish_speeds[i])
			fish.scale.x = 0.8

		# Bobbing
		fish.position.y += sin(Time.get_ticks_msec() / 500.0 + i) * 0.3

func _animate_title() -> void:
	var tween = create_tween().set_loops()
	tween.tween_property(title_label, "scale", Vector2(1.05, 1.05), 0.5)
	tween.tween_property(title_label, "scale", Vector2(1.0, 1.0), 0.5)
