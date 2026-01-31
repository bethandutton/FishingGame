extends Node2D

# Claw settings
@export var move_speed: float = 400.0
@export var drop_speed: float = 600.0
@export var raise_speed: float = 400.0
@export var min_y: float = 100.0
@export var max_y: float = 1100.0
@export var min_x: float = 50.0
@export var max_x: float = 670.0

# State machine - simplified
enum ClawState { IDLE, HOLDING }
var current_state: ClawState = ClawState.IDLE

# Touch state
var is_touching: bool = false

# Grabbed fish reference
var grabbed_fish: Node2D = null

# Node references
@onready var rope: Line2D = $Rope
@onready var claw_head: Node2D = $ClawHead
@onready var grab_area: Area2D = $ClawHead/GrabArea
@onready var claw_left: Polygon2D = $ClawHead/ClawLeft
@onready var claw_right: Polygon2D = $ClawHead/ClawRight

var initial_position: Vector2

func _ready() -> void:
	initial_position = position

	# Setup collision shape for grab area
	var shape = CircleShape2D.new()
	shape.radius = 30.0
	grab_area.get_node("CollisionShape2D").shape = shape

func _input(event: InputEvent) -> void:
	# Handle touch/click press and release
	if event is InputEventScreenTouch or event is InputEventMouseButton:
		var pressed = event.pressed if event is InputEventMouseButton else event.pressed

		if pressed:
			is_touching = true
			close_claw()
		else:
			is_touching = false
			# Try to grab a fish when releasing
			if not grabbed_fish:
				try_grab_fish()

	# Handle drag for horizontal movement
	elif event is InputEventScreenDrag:
		if is_touching:
			position.x += event.relative.x
			position.x = clamp(position.x, min_x, max_x)
	elif event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
		position.x += event.relative.x
		position.x = clamp(position.x, min_x, max_x)

func _process(delta: float) -> void:
	if is_touching:
		# HOLDING: Move claw DOWN while touching
		claw_head.position.y += drop_speed * delta

		# Clamp to max depth
		var max_depth = max_y - position.y
		if claw_head.position.y > max_depth:
			claw_head.position.y = max_depth
	else:
		# RELEASED: Move claw UP when not touching
		claw_head.position.y -= raise_speed * delta

		# Clamp to top
		if claw_head.position.y < 0:
			claw_head.position.y = 0

			# If we brought a fish up, enter holding state
			if grabbed_fish:
				current_state = ClawState.HOLDING
			else:
				current_state = ClawState.IDLE
				open_claw()

	# Update rope visual
	update_rope()

	# Keep grabbed fish attached to claw
	if grabbed_fish and is_instance_valid(grabbed_fish):
		grabbed_fish.global_position = grab_area.global_position

func update_rope() -> void:
	rope.points = PackedVector2Array([
		Vector2(0, -100),
		Vector2(0, claw_head.position.y)
	])

func try_grab_fish() -> void:
	var bodies = grab_area.get_overlapping_areas()
	for area in bodies:
		var parent = area.get_parent()
		if parent.is_in_group("fish") and not parent.is_grabbed:
			grab_fish(parent)
			break

func grab_fish(fish: Node2D) -> void:
	grabbed_fish = fish
	fish.grab()
	close_claw()

func release_fish() -> void:
	if grabbed_fish and is_instance_valid(grabbed_fish):
		grabbed_fish.release()
	grabbed_fish = null
	open_claw()
	current_state = ClawState.IDLE

func drop_fish_in_bucket() -> void:
	if grabbed_fish and is_instance_valid(grabbed_fish):
		grabbed_fish.queue_free()
	grabbed_fish = null
	open_claw()
	current_state = ClawState.IDLE

func open_claw() -> void:
	claw_left.rotation_degrees = -15
	claw_right.rotation_degrees = 15

func close_claw() -> void:
	claw_left.rotation_degrees = 5
	claw_right.rotation_degrees = -5

func reset() -> void:
	position = initial_position
	claw_head.position = Vector2.ZERO
	current_state = ClawState.IDLE
	is_touching = false
	if grabbed_fish and is_instance_valid(grabbed_fish):
		grabbed_fish.queue_free()
	grabbed_fish = null
	open_claw()
	update_rope()

func has_fish() -> bool:
	return grabbed_fish != null and is_instance_valid(grabbed_fish)

func get_grabbed_fish_id() -> int:
	if grabbed_fish and is_instance_valid(grabbed_fish) and grabbed_fish.has_meta("fish_id"):
		return grabbed_fish.get_meta("fish_id")
	return -1
