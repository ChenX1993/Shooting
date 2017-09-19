using UnityEngine;
using System.Collections;

public class PlayerAnimationListener : MonoBehaviour {

    private TransformPathMaker pathMaker;
    private PlayerBehaviour playerBehaviour;

    void Start()
    {
        pathMaker = transform.parent.GetComponent<TransformPathMaker>();
        playerBehaviour = pathMaker.gameObject.GetComponent<PlayerBehaviour>();
       
    }
    public void PlayPathMaker()
    {
        pathMaker.Play();
    }
    public void ResetPathMaker()
    {
        pathMaker.Reset();
    }
	public void NextClimbState()
    {
        pathMaker.NextState();
    }
    public void Jump()
    {
        playerBehaviour.Jump();
    }
}
