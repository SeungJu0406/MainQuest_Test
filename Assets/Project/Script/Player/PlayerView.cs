using Fusion;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    public enum State
    {

    }

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayAnimation(string anim)
    {
        if (_animator == null) return;
        _animator.Play(anim);
    }
}
