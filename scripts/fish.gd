extends Node2D

# Fish movement settings
@export var swim_speed: float = 80.0
@export var swim_range: float = 60.0

# State
var is_grabbed: bool = false
var swim_direction: int = 1
var start_x: float
var time_offset: float

func _ready() -> void:
	start_x = position.x
	time_offset = randf() * 10.0  # Random phase offset

	# Use synced values if available (multiplayer), otherwise randomize
	if has_meta("synced_speed"):
		swim_speed = get_meta("synced_speed")
	else:
		swim_speed = randf_range(50.0, 120.0)

	if has_meta("synced_direction"):
		swim_direction = get_meta("synced_direction")
	else:
		swim_direction = 1 if randf() > 0.5 else -1

func _process(delta: float) -> void:
	if is_grabbed:
		return
	
	# Swim back and forth with slight bobbing
	var time = Time.get_ticks_msec() / 1000.0 + time_offset
	
	# Horizontal swimming
	position.x += swim_direction * swim_speed * delta
	
	# Reverse direction at boundaries
	if position.x > start_x + swim_range:
		swim_direction = -1
		flip_fish(-1)
	elif position.x < start_x - swim_range:
		swim_direction = 1
		flip_fish(1)
	
	# Gentle vertical bobbing
	var base_y = get_meta("base_y") if has_meta("base_y") else 0
	position.y = base_y + sin(time * 2.0) * 5.0

func flip_fish(direction: int) -> void:
	# Flip fish to face swimming direction
	scale.x = direction

func grab() -> void:
	is_grabbed = true
	# Visual feedback - fish wiggles when grabbed
	var tween = create_tween()
	tween.tween_property(self, "rotation", 0.2, 0.1)
	tween.tween_property(self, "rotation", -0.2, 0.1)
	tween.tween_property(self, "rotation", 0.0, 0.1)

func release() -> void:
	is_grabbed = false
	# Return to water
	start_x = position.x
