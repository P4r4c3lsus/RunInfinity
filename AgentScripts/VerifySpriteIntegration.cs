using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RunInfinity.Adicional;

public static class VerifySpriteIntegration
{
    public static async Task<object> Main()
    {
        var game = UnityEngine.Object.FindAnyObjectByType<RIGame>();
        var driver = game.jugador.GetComponentInChildren<RICharacterAnimation>();
        var animator = driver.GetComponent<Animator>();
        var renderer = driver.GetComponent<SpriteRenderer>();
        var states = new HashSet<string>();
        var runSprites = new HashSet<string>();
        int firstFrame = Time.frameCount;
        game.StartGame();
        float started = Time.time;
        while (Time.time - started < 0.3f)
        {
            if (renderer.sprite != null) runSprites.Add(renderer.sprite.name);
            await Task.Delay(5);
        }
        game.jugador.RequestJump();
        float jumpStart = Time.time;
        while (Time.time - jumpStart < 1.2f)
        {
            foreach (string state in new[] { "Idle", "Run", "Jump", "Fall", "Land" })
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(state)) states.Add(state);
            await Task.Delay(5);
        }
        if (Time.frameCount <= firstFrame) throw new Exception("The player loop is frozen.");
        if (runSprites.Count < 2) throw new Exception("Running animation does not change sprites.");
        foreach (string state in new[] { "Run", "Jump", "Fall", "Land" })
            if (!states.Contains(state)) throw new Exception("Animation state was not observed: " + state);
        game.Pause();
        await Task.Delay(40);
        string pausedSprite = renderer.sprite.name;
        float pausedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        await Task.Delay(100);
        if (renderer.sprite.name != pausedSprite || Math.Abs(animator.GetCurrentAnimatorStateInfo(0).normalizedTime - pausedTime) > 0.001f)
            throw new Exception("Animation advances while paused.");
        game.Resume();
        typeof(RIGame).GetMethod("Spawn", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(game, null);
        var obstacles = UnityEngine.Object.FindObjectsByType<RIObstacle>();
        int checkedObstacles = 0;
        foreach (var obstacle in obstacles)
        {
            if (!obstacle.gameObject.activeInHierarchy) continue;
            var appearance = obstacle.GetComponentInChildren<RIObstacleAppearance>();
            if (appearance == null || appearance.variants.Length != 8 || appearance.GetComponent<SpriteRenderer>().sprite == null)
                throw new Exception("Spawned obstacle has no rocky sprite variants.");
            checkedObstacles++;
        }
        if (checkedObstacles == 0) throw new Exception("No spawned obstacles to verify.");
        game.Pause();
        var visible = obstacles[obstacles.Length - 1];
        visible.transform.position = new Vector3(game.jugador.transform.position.x + 2.5f,
            game.suelo.bounds.max.y + 0.3f, 0);
        return new { passed = true, firstFrame, lastFrame = Time.frameCount,
            runningFramesObserved = runSprites.Count, states = string.Join(", ", states),
            pauseFreezesAnimation = true, checkedObstacles };
    }
}
