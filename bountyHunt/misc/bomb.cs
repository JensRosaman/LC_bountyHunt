namespace bountyHunt.misc;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class Explosion : NetworkBehaviour, IHittable
{
	private bool mineActivated = true;

	public bool hasExploded;

	public ParticleSystem explosionParticle;
	
	private bool sendingExplosionRPC;

	private RaycastHit hit;

	private RoundManager roundManager;

	private float pressMineDebounceTimer;

	private bool localPlayerOnMine;

	private void Start()
	{
		
	}

	private void Update()
	{
		
	}
	

	[ServerRpc(RequireOwnership = false)]
	public void ToggleMineServerRpc(bool enable)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams = default(ServerRpcParams);
				FastBufferWriter bufferWriter = __beginSendServerRpc(2763604698u, serverRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in enable, default(FastBufferWriter.ForPrimitives));
				__endSendServerRpc(ref bufferWriter, 2763604698u, serverRpcParams, RpcDelivery.Reliable);
			}
			
		}
	}
	
	public void TriggerMineOnLocalClientByExiting()
	{
		
		
			//SetOffMineAnimation();
			sendingExplosionRPC = true;
			ExplodeMineServerRpc();
		
	}

	[ServerRpc(RequireOwnership = false)]
	public void ExplodeMineServerRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams = default(ServerRpcParams);
				FastBufferWriter bufferWriter = __beginSendServerRpc(3032666565u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 3032666565u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
			{
				ExplodeMineClientRpc();
			}
		}
	}

	[ClientRpc]
	public void ExplodeMineClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(456724201u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 456724201u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
		{
			if (sendingExplosionRPC)
			{
				sendingExplosionRPC = false;
			}
			else
			{
				SetOffMineAnimation();
			}
		}
	}
	
	public void SetOffMineAnimation()
	{
		hasExploded = true;
	}
	
	public void Detonate(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f)
	{
		SpawnExplosion(explosionPosition, spawnExplosionEffect ,    1f,  1f);
	}

	public static void SpawnExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f)
	{
		Debug.Log($"Spawning explosion at pos: {explosionPosition.ToString()}");
		if (spawnExplosionEffect)
		{
			Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform).SetActive(value: true);
		}
		float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
		if (num < 14f)
		{
			HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
		}
		else if (num < 25f)
		{
			HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
		}
		Collider[] array = Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);
		PlayerControllerB playerControllerB = null;
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
			if (num2 > 4f && Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
			{
				continue;
			}
			if (array[i].gameObject.layer == 3)
			{
				playerControllerB = array[i].gameObject.GetComponent<PlayerControllerB>();
				if (playerControllerB != null && playerControllerB.IsOwner)
				{
					if (num2 < killRange)
					{
						Vector3 bodyVelocity = (playerControllerB.gameplayCamera.transform.position - explosionPosition) * 80f / Vector3.Distance(playerControllerB.gameplayCamera.transform.position, explosionPosition);
						playerControllerB.KillPlayer(bodyVelocity, spawnBody: true, CauseOfDeath.Blast);
					}
					else if (num2 < damageRange)
					{
						playerControllerB.DamagePlayer(50);
					}
				}
			}
			else if (array[i].gameObject.layer == 21)
			{
				Landmine componentInChildren = array[i].gameObject.GetComponentInChildren<Landmine>();
				if (componentInChildren != null && !componentInChildren.hasExploded && num2 < 6f)
				{
					Debug.Log("Setting off other mine");
					
				}
			}
			else if (array[i].gameObject.layer == 19)
			{
				EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
				if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
				{
					componentInChildren2.mainScript.HitEnemyOnLocalClient(6);
				}
			}
		}
		int num3 = ~LayerMask.GetMask("Room");
		num3 = ~LayerMask.GetMask("Colliders");
		array = Physics.OverlapSphere(explosionPosition, 10f, num3);
		for (int j = 0; j < array.Length; j++)
		{
			Rigidbody component = array[j].GetComponent<Rigidbody>();
			if (component != null)
			{
				component.AddExplosionForce(70f, explosionPosition, 10f);
			}
		}
	}

	public bool MineHasLineOfSight(Vector3 pos)
	{
		return !Physics.Linecast(base.transform.position, pos, out hit, 256);
	}

	void IHittable.Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX)
	{
		SetOffMineAnimation();
		sendingExplosionRPC = true;
		ExplodeMineServerRpc();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	/*[RuntimeInitializeOnLoadMethod]
	internal static void InitializeRPCS_Landmine()
	{
		NetworkManager.__rpc_func_table.Add(2763604698u, __rpc_handler_2763604698);
		NetworkManager.__rpc_func_table.Add(3479956057u, __rpc_handler_3479956057);
		NetworkManager.__rpc_func_table.Add(4224840819u, __rpc_handler_4224840819);
		NetworkManager.__rpc_func_table.Add(2652432181u, __rpc_handler_2652432181);
		NetworkManager.__rpc_func_table.Add(3032666565u, __rpc_handler_3032666565);
		NetworkManager.__rpc_func_table.Add(456724201u, __rpc_handler_456724201);
	}

	private static void __rpc_handler_2763604698(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Server;
			((Landmine)target).ToggleMineServerRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.None;
		}
	}

	private static void __rpc_handler_3479956057(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Client;
			
			target.__rpc_exec_stage = __RpcExecStage.None;
		}
	}

	private static void __rpc_handler_4224840819(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Server;
			((Landmine)target).PressMineServerRpc();
			target.__rpc_exec_stage = __RpcExecStage.None;
		}
	}

	private static void __rpc_handler_2652432181(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Client;
			((Landmine)target).PressMineClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.None;
		}
	}

	private static void __rpc_handler_3032666565(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Server;
			((Landmine)target).ExplodeMineServerRpc();
			target.__rpc_exec_stage = __RpcExecStage.None;
		}
	}

	private static void __rpc_handler_456724201(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Client;
			((Landmine)target).ExplodeMineClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.None;
		}
	}
	*/

	
}
