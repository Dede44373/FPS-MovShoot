using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnEnemy : MonoBehaviour
{
    public UserInputs controls;
    public GameObject enemy;
    public Transform spawnPoint;
    private void OnEnable()
    {
        controls = UserInputManager.Instance.Controls;
        controls.Player.Temp.started += EnemySpawning;
    }

    private void OnDisable()
    {
        controls.Player.Temp.started -= EnemySpawning;
    }
    private void EnemySpawning(InputAction.CallbackContext ctx)
    {
        Instantiate(enemy, spawnPoint.transform.position, Quaternion.identity);
    }

   
}
