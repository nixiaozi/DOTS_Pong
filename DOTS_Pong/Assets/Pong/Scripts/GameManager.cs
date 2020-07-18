using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager main;

	public GameObject ballPrefab;

	public float xBound = 3f;
	public float yBound = 3f;
	public float ballSpeed = 3f;
	public float respawnDelay = 2f;
	public int[] playerScores;

	public Text mainText;
	public Text[] playerTexts;

	Entity ballEntityPrefab;
	EntityManager manager;

	WaitForSeconds oneSecond;
	WaitForSeconds delay;

	private void Awake()
	{
		// 如果当前main不等于空并且不为当前对象，就直接返回
		// 防止重复的对象定义
		if (main != null && main != this)
		{
			Destroy(gameObject);
			return;
		}

		main = this;
		playerScores = new int[2];

		// 获取默认世界的对象管理器
		manager = World.DefaultGameObjectInjectionWorld.EntityManager;

		//获取默认世界的配置，并且初始化对象ballPrefab到场景世界中
		GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
		ballEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(ballPrefab, settings); // 由于这里本身没有

		oneSecond = new WaitForSeconds(1f);  // 定义常量，一秒的等待时间
		delay = new WaitForSeconds(respawnDelay);  //获取和定义复位时间

		StartCoroutine(CountdownAndSpawnBall()); // 开始协程
	}

	public void PlayerScored(int playerID)
	{
		playerScores[playerID]++;
		for (int i = 0; i < playerScores.Length && i < playerTexts.Length; i++)
			playerTexts[i].text = playerScores[i].ToString();

		StartCoroutine(CountdownAndSpawnBall());
	}

	IEnumerator CountdownAndSpawnBall()
	{
		mainText.text = "Get Ready";
		yield return delay; // 这里的感觉是协程在这里等待的时间数，到时间后会继续向下执行。

		mainText.text = "3";
		yield return oneSecond;

		mainText.text = "2";
		yield return oneSecond;

		mainText.text = "1";
		yield return oneSecond;

		mainText.text = "";

		SpawnBall();
	}

	void SpawnBall()  // 重新生成球体对象
	{
		Entity ball = manager.Instantiate(ballEntityPrefab); // 实例化entity对象

		Vector3 dir = new Vector3(UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1, UnityEngine.Random.Range(-.5f, .5f), 0f).normalized;
		Vector3 speed = dir * ballSpeed;

		PhysicsVelocity velocity = new PhysicsVelocity()
		{
			Linear = speed, // 线速度位移
			Angular = float3.zero // 角速度 旋转
		};

		manager.AddComponentData(ball, velocity);
	}
}

