extends Node2D

# Spawner settings
@export var num_fish: int = 15
@export var spawn_width: float = 620.0
@export var spawn_height: float = 250.0

# Fish tracking
var fish_scene: PackedScene
var active_fish: Array[Node2D] = []
var fish_id_counter: int = 0

# Fish colors matching the real game
var fish_colors: Array[Color] = [
	Color(1.0, 0.3, 0.3),   # Red
	Color(1.0, 0.6, 0.2),   # Orange
	Color(1.0, 0.9, 0.2),   # Yellow
	Color(0.3, 0.8, 0.3),   # Green
	Color(0.3, 0.5, 1.0),   # Blue
	Color(0.6, 0.3, 0.8),   # Purple
]

func _ready() -> void:
	# Create fish scene programmatically
	spawn_initial_fish()

func spawn_initial_fish() -> void:
	for i in range(num_fish):
		spawn_fish(i)

func spawn_fish(index: int) -> void:
	var fish = create_fish_node(index)

	# Position fish in a grid-like pattern with some randomness
	var row = index / 5
	var col = index % 5
	var base_x = 60 + col * 140 + randf_range(-20, 20)
	var base_y = row * 80 + randf_range(-10, 10)

	fish.position = Vector2(base_x, base_y)
	fish.set_meta("base_y", base_y)
	fish.set_meta("fish_id", fish_id_counter)
	fish_id_counter += 1

	add_child(fish)
	active_fish.append(fish)

func create_fish_node(index: int) -> Node2D:
	var fish = Node2D.new()
	fish.name = "Fish_" + str(index)
	fish.add_to_group("fish")
	
	# Add fish script
	var script = load("res://scripts/fish.gd")
	fish.set_script(script)
	
	# Fish body - round like in the real game
	var body = Polygon2D.new()
	body.name = "Body"
	body.color = fish_colors[index % fish_colors.size()]
	body.polygon = create_fish_body_polygon()
	fish.add_child(body)
	
	# Fish eye - big googly eye like the toy
	var eye_white = Polygon2D.new()
	eye_white.name = "EyeWhite"
	eye_white.color = Color(1, 1, 1)
	eye_white.polygon = create_circle_polygon(8, 10)
	eye_white.position = Vector2(5, -8)
	fish.add_child(eye_white)
	
	var eye_pupil = Polygon2D.new()
	eye_pupil.name = "EyePupil"
	eye_pupil.color = Color(0, 0, 0)
	eye_pupil.polygon = create_circle_polygon(4, 8)
	eye_pupil.position = Vector2(7, -8)
	fish.add_child(eye_pupil)
	
	# Mouth area (opens when fish can be caught)
	var mouth = Polygon2D.new()
	mouth.name = "Mouth"
	mouth.color = Color(0.15, 0.1, 0.1)
	mouth.polygon = create_circle_polygon(10, 8)
	mouth.position = Vector2(0, 12)
	fish.add_child(mouth)
	
	# Collision area for grabbing
	var area = Area2D.new()
	area.name = "GrabArea"
	var collision = CollisionShape2D.new()
	var shape = CircleShape2D.new()
	shape.radius = 25.0
	collision.shape = shape
	area.add_child(collision)
	fish.add_child(area)
	
	return fish

func create_fish_body_polygon() -> PackedVector2Array:
	# Round fish body like the toy
	var points = PackedVector2Array()
	var segments = 12
	for i in range(segments):
		var angle = (2 * PI * i) / segments
		var radius_x = 28.0
		var radius_y = 25.0
		# Make it slightly pointy at front
		if i >= 9 or i <= 3:
			radius_x *= 0.9
		points.append(Vector2(cos(angle) * radius_x, sin(angle) * radius_y))
	return points

func create_circle_polygon(radius: float, segments: int) -> PackedVector2Array:
	var points = PackedVector2Array()
	for i in range(segments):
		var angle = (2 * PI * i) / segments
		points.append(Vector2(cos(angle), sin(angle)) * radius)
	return points

func reset_fish() -> void:
	# Remove all existing fish
	for fish in active_fish:
		if is_instance_valid(fish):
			fish.queue_free()
	active_fish.clear()
	
	# Spawn new fish
	spawn_initial_fish()

func _process(_delta: float) -> void:
	# Respawn fish if too few remain
	var valid_fish = 0
	for fish in active_fish:
		if is_instance_valid(fish):
			valid_fish += 1
	
	if valid_fish < 5:
		var new_index = active_fish.size()
		spawn_fish(new_index)
