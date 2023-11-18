using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAnimationController : MonoBehaviour {

    public BlockAnimationData animationData;
    public ChunkController rootChunk;
    public int rootBIdx;
    public float frameProgress;
    public bool playing;
    public int frame;

    void Start()
    {
        frameProgress = 0;
        if (animationData.playOnStart)
        {
            playing = true;

            if (animationData.startOnRandomFrame) frame = Random.Range(0, animationData.frames.Length);
        }
    }

    void Update()
    {
        if (playing)
        {
            frameProgress += Time.deltaTime;

            if (frameProgress > animationData.timePerFrame)
            {
                frameProgress = 0;
                frame++;
                if (frame >= animationData.frames.Length)
                {
                    frame = 0;

                    if (!animationData.loop)
                    {
                        playing = false;
                    }
                }
            }
        }

        if (rootChunk.TileGrid[rootBIdx] != animationData.rootTile)
        {
            Destroy(this);
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().sprite = animationData.frames[frame];
        }
    }

    public void PlayAnimation()
    {
        playing = true;
    }

    public void GotoFrame(int frame)
    {
        this.frame = frame;
        transform.GetComponent<SpriteRenderer>().sprite = animationData.frames[frame];
    }

    public void StopAnimation()
    {
        playing = false;
    }
}
