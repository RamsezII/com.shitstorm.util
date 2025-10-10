using UnityEngine;

partial class Util
{
    public static void AddTorqueAt(this Rigidbody rigidbody, in Vector3 torque, in Vector3 pivot, in ForceMode mode)
    {
        Vector3 wcog = rigidbody.worldCenterOfMass;

        Vector3 local = Quaternion.Inverse(rigidbody.rotation) * (pivot - wcog);
        Vector3 wpos2 = wcog + Quaternion.Euler(Mathf.Rad2Deg * Time.fixedDeltaTime * torque) * rigidbody.rotation * local;

        rigidbody.AddTorque(torque, mode);
        rigidbody.AddForce((pivot - wpos2) / Time.fixedDeltaTime, mode);
    }
}
