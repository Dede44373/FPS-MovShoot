using UnityEngine;

[CreateAssetMenu(menuName = "Player/MovementData")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Base Speed Values")]
    public float walkSpeed;
    public float sprintSpeed;

    public float airControlModifier = 4;
    public float groundControlModifier = 10;
    public float groundDrag;
    public float airDrag;
    public float slideDrag;
    public float addGravity;

    [Header("Jump Values")]
    public float jumpForce;
    public float jumpCooldown;
    [Tooltip("Number of total jumps, eg 0 means you cant jump")]
    public int baseJumpUses = 2;
}
