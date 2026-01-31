extends Area2D

signal fish_dropped

@onready var claw: Node2D = get_parent().get_node("Claw")

func _ready() -> void:
	# Setup collision shape
	var shape = RectangleShape2D.new()
	shape.size = Vector2(120, 120)
	$CollisionShape2D.shape = shape
	
	# Connect area signals
	area_entered.connect(_on_area_entered)

func _on_area_entered(area: Area2D) -> void:
	# Check if it's the claw's grab area with a fish
	if area.name == "GrabArea" and claw.has_fish():
		# Fish successfully dropped in bucket!
		claw.drop_fish_in_bucket()
		fish_dropped.emit()
		
		# Visual feedback
		show_score_popup()

func show_score_popup() -> void:
	# Create a "+1" popup
	var popup = Label.new()
	popup.text = "+1 🐟"
	popup.add_theme_font_size_override("font_size", 32)
	popup.position = Vector2(-30, -100)
	popup.modulate = Color(0.2, 1.0, 0.4)
	add_child(popup)
	
	# Animate it floating up and fading
	var tween = create_tween()
	tween.set_parallel(true)
	tween.tween_property(popup, "position:y", -150, 0.5)
	tween.tween_property(popup, "modulate:a", 0.0, 0.5)
	tween.chain().tween_callback(popup.queue_free)
