using Godot;

public partial class CameraController : Node3D
{
	// how far the camera moves in meters with every zoom button press
	private const float CAM_STEP = 1;
	// camera distance boundaries are measured in meters
	private const float CAM_DIST_MIN = 5;
	private const float CAM_DIST_MAX = 20;
	// lower value means that the camera will move slower when moving the mouse
	private const float MOUSE_SENSITIVITY = 0.1f;
	// camera pitch boundaries are measured in degrees
	private const int MAX_PITCH = 89;
	private const int MIN_PITCH = -89;

	// important nodes
	private Node3D camYaw;
	private Node3D camPitch;
	private SpringArm3D camArm;

	public override void _Ready()
	{
		// assign important nodes
		camYaw = (Node3D)GetNode("Yaw");
		camPitch = (Node3D)GetNode("Yaw/Pitch");
		camArm = (SpringArm3D)GetNode("Yaw/Pitch/SpringArm3D");

		// capture the mouse on startup
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _Input(InputEvent ie)
    {
		if(ie.IsActionPressed("toggle_mouse_capture"))
		{
			if(Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}

		// camera mouse movement
		if(Input.MouseMode == Input.MouseModeEnum.Captured
			&& ie is InputEventMouseMotion iemm)
		{
			camYaw.RotateY(Mathf.DegToRad(-iemm.Relative.X * MOUSE_SENSITIVITY));
			camPitch.RotateX(Mathf.DegToRad(-iemm.Relative.Y * MOUSE_SENSITIVITY));
			// prevent camera from pitching too high or too low
			Vector3 newPitch = camPitch.Rotation;
			newPitch.X = Mathf.Clamp(newPitch.X, Mathf.DegToRad(MIN_PITCH), Mathf.DegToRad(MAX_PITCH));
			camPitch.Rotation = newPitch;
		}

		// camera zoom functionality
		if(ie.IsActionPressed("zoom_in"))
		{
			ChangeCamArmLength(-CAM_STEP);
		}
		if(ie.IsActionPressed("zoom_out"))
		{
			ChangeCamArmLength(CAM_STEP);
		}
    }

	/**
	 * Adjust camera distance by the given amount.
	 * Param amount: the camera will move this distance away from the look-at point.
	 */
	private void ChangeCamArmLength(float amount)
	{
		float newLength = camArm.SpringLength + amount;
		// ensure the camera does not get too far or too close
		newLength = Mathf.Clamp(newLength, CAM_DIST_MIN, CAM_DIST_MAX);
		camArm.SpringLength = newLength;
	}
}
