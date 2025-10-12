using UnityEngine;

partial class Util
{
    public static void AddVelocityTorqueAt(this Rigidbody rigidbody, in Vector3 torque, in Vector3 pivot)
    {
        if (torque == default)
            return;

        Vector3 wcog = rigidbody.worldCenterOfMass;
        Quaternion rot = rigidbody.rotation;

        Vector3 local = Quaternion.Inverse(rot) * (pivot - wcog);
        Vector3 wpos2 = wcog + Quaternion.Euler(Mathf.Rad2Deg * torque) * rot * local;
        Vector3 error = pivot - wpos2;

        rigidbody.AddTorque(torque, ForceMode.VelocityChange);

        if (error != default)
            rigidbody.AddForce(error, ForceMode.VelocityChange);
    }
}
