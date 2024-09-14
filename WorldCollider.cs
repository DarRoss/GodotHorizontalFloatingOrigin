using Godot;

public partial class WorldCollider : AnimatableBody3D
{
    // floated vehicles seem to ignore friction and drag, so we apply our own
    private const float HORIZ_DRAG = 0.05f;

    // This vehicle acts as the center of the universe.
    [Export]
    private CarController anchorVehicle;

    // signals necessary for HUD
    [Signal]
    public delegate void SpeedSignalEventHandler(float speed);
    [Signal]
    public delegate void FloatingOriginSignalEventHandler(bool isOriginFloating);
    [Signal]
    public delegate void PositionsSignalEventHandler(Vector2 truePos, Vector2 virtPos);

    // true Vector3 to virtual Vector2 conversion: true -Z becomes virtual +Y
    private Vector2 anchorVirtualVel = Vector2.Zero;
    private Vector2 anchorVirtualPos = Vector2.Zero;

    private bool isOriginFloating = false;

    public override void _Process(double delta)
    {
        if(isOriginFloating)
        {
            UpdateAnchorVirtualHorizontalVelocity();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // apply drag
        anchorVirtualVel *= 1 - (HORIZ_DRAG * (float)delta);
        anchorVirtualPos += anchorVirtualVel * (float)delta;
        // move the world like an underfoot treadmill
        GlobalPosition = -new Vector3(anchorVirtualPos.X, 0, -anchorVirtualPos.Y);
        // send speed and position signals to update hud
        SendSpeedSignal();
        SendPositionsSignal();
    }

    public override void _Input(InputEvent ie)
    {
		if(ie.IsActionPressed("toggle_floating_origin"))
		{
            ToggleFloatingOrigin();
		}
        if(ie.IsActionPressed("reset_car"))
        {
            anchorVirtualPos = Vector2.Zero;
            anchorVirtualVel = Vector2.Zero;
        }
    }

    private void SendSpeedSignal()
    {
        float speed;
        if(isOriginFloating)
        {
            // calculate speed from virtual linear velocity
            speed = anchorVirtualVel.Length();
        }
        else
        {
            // calculate speed from true linear velocity
            speed = new Vector2(anchorVehicle.LinearVelocity.X, -anchorVehicle.LinearVelocity.Z).Length();
        }
        // speed is sent as a measure of meters per second
        EmitSignal(SignalName.SpeedSignal, speed);
    }
    
    private void SendPositionsSignal()
    {
        // y axis is unecessary, so convert horizontal global position into a vector2
        Vector2 anchorTruePos = new(anchorVehicle.GlobalPosition.X, -anchorVehicle.GlobalPosition.Z);
        EmitSignal(SignalName.PositionsSignal, anchorTruePos, anchorVirtualPos);
    }

    private void UpdateAnchorVirtualHorizontalVelocity()
    {
        // check for true vehicle movement on the x axis
        if(!Mathf.IsZeroApprox(anchorVehicle.LinearVelocity.X))
        {
            anchorVirtualVel.X += anchorVehicle.LinearVelocity.X;
            anchorVehicle.LinearVelocity = new Vector3(
                0, anchorVehicle.LinearVelocity.Y, anchorVehicle.LinearVelocity.Z);
        }
        // check for true vehicle movement on the z axis
        if(!Mathf.IsZeroApprox(anchorVehicle.LinearVelocity.Z))
        {
            // true -Z becomes virtual +Y
            anchorVirtualVel.Y -= anchorVehicle.LinearVelocity.Z;
            anchorVehicle.LinearVelocity = new Vector3(
                anchorVehicle.LinearVelocity.X, anchorVehicle.LinearVelocity.Y, 0);
        }
    }

    private void ToggleFloatingOrigin()
    {
        isOriginFloating = !isOriginFloating;

        if(isOriginFloating)
        {
            // convert true position into virtual position
            anchorVirtualPos = new Vector2(anchorVehicle.GlobalPosition.X, -anchorVehicle.GlobalPosition.Z);
            anchorVehicle.IntegGlobalPosition = new Vector3(0, anchorVehicle.GlobalPosition.Y, 0);
        }
        else
        {
            // convert virtual position into true position
            anchorVehicle.IntegGlobalPosition = new Vector3(
                anchorVirtualPos.X, anchorVehicle.GlobalPosition.Y, -anchorVirtualPos.Y);
            anchorVirtualPos = Vector2.Zero;
            // convert virtual velocity into true linear velocity
            anchorVehicle.IntegLinearVelocity = new Vector3(
                anchorVirtualVel.X, anchorVehicle.LinearVelocity.Y, -anchorVirtualVel.Y);
            anchorVirtualVel = Vector2.Zero;
        }

        EmitSignal(SignalName.FloatingOriginSignal, isOriginFloating);
    }
}