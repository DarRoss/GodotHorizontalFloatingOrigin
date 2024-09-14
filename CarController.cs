using Godot;

public partial class CarController : VehicleBody3D
{
	private const float ENGINE_MAX_FORCE = 10000;
	private const float BRAKE_MAX_FORCE = 300;
	private const float STEER_ANGLE = 5;

	// important nodes
	private VehicleWheel3D wheelBackLeft;
	private VehicleWheel3D wheelBackRight;

	private bool isResetCar = false;
	// vectors for use in IntegrateForces
	public Vector3 IntegLinearVelocity{get; set;} = Vector3.Inf;
	public Vector3 IntegGlobalPosition{get; set;} = Vector3.Inf;

    public override void _Ready()
    {
		// assign important nodes
		wheelBackLeft = (VehicleWheel3D)GetNode("WheelBL");
		wheelBackRight = (VehicleWheel3D)GetNode("WheelBR");
    }

    public override void _Input(InputEvent ie)
    {
		if(ie.IsAction("forward"))
		{
			EngineForce = ENGINE_MAX_FORCE * Input.GetActionStrength("forward");
		}
		if(ie.IsAction("backward"))
		{
			wheelBackRight.Brake = BRAKE_MAX_FORCE * Input.GetActionStrength("backward");
			wheelBackLeft.Brake = BRAKE_MAX_FORCE * Input.GetActionStrength("backward");
		}
		if(ie.IsAction("right"))
		{
			Steering = Mathf.DegToRad(-STEER_ANGLE) * Input.GetActionStrength("right");
		}
		if(ie.IsAction("left"))
		{
			Steering = Mathf.DegToRad(STEER_ANGLE) * Input.GetActionStrength("left");
		}
		if(ie.IsActionPressed("reset_car"))
		{
			isResetCar = true;
		}
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
		// check if we desire a new position
		if(!IntegGlobalPosition.IsEqualApprox(Vector3.Inf))
		{
			GlobalPosition = IntegGlobalPosition;
			IntegGlobalPosition = Vector3.Inf;
		}
		// check if we desire a new linear velocity
		if(!IntegLinearVelocity.IsEqualApprox(Vector3.Inf))
		{
			LinearVelocity = IntegLinearVelocity;
			IntegLinearVelocity = Vector3.Inf;
		}
		// check if car needs to be reset
        if(isResetCar)
		{
			isResetCar = false;
			GlobalPosition = new Vector3(0, 1, 0);
			GlobalRotation = Vector3.Zero;
			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
		}
    }
}
