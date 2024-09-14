using Godot;

public partial class Hud : Control
{
	[Export]
	private WorldCollider worldCollider;
	private Label speedometer;
	private Label floatingOriginIndicator;
	private Label coordinates;

    public override void _Ready()
    {
		speedometer = (Label)GetNode("MarginContainer/HBoxContainer/BoxesLeft/RectSpeed/Speed");
		floatingOriginIndicator = (Label)GetNode("MarginContainer/HBoxContainer/BoxesCenter/RectFOI/FOI");
		coordinates = (Label)GetNode("MarginContainer/HBoxContainer/BoxesLeft/RectCoords/Coords");

		ConnectSignals();
    }

	private void ConnectSignals()
	{
		worldCollider.SpeedSignal += OnReceiveSpeed;
		worldCollider.PositionsSignal += OnReceivePositions;
		worldCollider.FloatingOriginSignal += OnReceiveFOI;
	}

	public void OnReceiveSpeed(float speed)
	{
		// convert meters per second to kilometers per hour
		speedometer.Text = "Speed: " + (int)(speed * 3.6f) + " km/h";
	}

	public void OnReceiveFOI(bool isOriginFloating)
	{
		string status = isOriginFloating ? "Enabled" : "Disabled";
		floatingOriginIndicator.Text = "Floating Origin: " + status;
	}

	public void OnReceivePositions(Vector2 truePos, Vector2 virtPos)
	{
		// converting Vector2 to Vector3 means that +Y becomes -Z
		coordinates.Text = 
			"True X: \t" + (int)truePos.X + 
			"\nTrue Z: \t" + (int)-truePos.Y +
			"\nVirtual X: \t" + (int)virtPos.X +
			"\nVirtual Z: \t" + (int)-virtPos.Y;
	}
}
