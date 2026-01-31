extends Node2D

## Visual representation of opponent's claw (no physics/interaction)

# Claw state enum (must match claw.gd)
enum ClawState { IDLE, HOLDING }

# Node references
@onready var rope: Line2D = $Rope
@onready var claw_head: Node2D = $ClawHead
@onready var claw_left: Polygon2D = $ClawHead/ClawLeft
@onready var claw_right: Polygon2D = $ClawHead/ClawRight
@onready var fish_indicator: Node2D = $ClawHead/FishIndicator
@onready var name_label: Label = $NameLabel

# Smooth movement
var target_position: Vector2 = Vector2.ZERO
var target_head_y: float = 0.0
var current_state: int = ClawState.IDLE
var has_fish: bool = false

const LERP_SPEED: float = 15.0

func _ready() -> void:
	# Set initial position
	target_position = position

	# Make the claw semi-transparent to distinguish from local player
	modulate = Color(1, 1, 1, 0.6)

	# Hide fish indicator initially
	if fish_indicator:
		fish_indicator.visible = false

func _process(delta: float) -> void:
	# Smoothly interpolate to target position
	position = position.lerp(target_position, LERP_SPEED * delta)
	claw_head.position.y = lerp(claw_head.position.y, target_head_y, LERP_SPEED * delta)

	# Update rope
	_update_rope()

	# Update claw open/close based on state
	_update_claw_visual()

func update_from_network(pos: Vector2, head_y: float, state: int, holding_fish: bool) -> void:
	target_position = pos
	target_head_y = head_y
	current_state = state
	has_fish = holding_fish

	# Update fish indicator
	if fish_indicator:
		fish_indicator.visible = has_fish

func _update_rope() -> void:
	if rope:
		rope.points = PackedVector2Array([
			Vector2(0, -100),
			Vector2(0, claw_head.position.y)
		])

func _update_claw_visual() -> void:
	# State 0 = ascending (open claw), State 1 = descending (closed claw)
	var is_open = current_state == 0 and not has_fish

	if claw_left and claw_right:
		if is_open:
			claw_left.rotation_degrees = -15
			claw_right.rotation_degrees = 15
		else:
			claw_left.rotation_degrees = 5
			claw_right.rotation_degrees = -5

func set_opponent_name(opponent_name: String) -> void:
	if name_label:
		name_label.text = opponent_name
