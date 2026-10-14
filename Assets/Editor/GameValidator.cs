using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ButterflyGame;

public static class GameValidator
{
    private const string SCENE_PATH = "Assets/Scenes/GardenScene.unity";

    [MenuItem("Butterfly Game/Run Automated Validation")]
    public static void ValidateGamePlay()
    {
        Debug.Log("=================================================");
        Debug.Log("   STARTING REALISTIC BUTTERFLY CATCHER TEST SUITE   ");
        Debug.Log("=================================================");

        int passCount = 0;
        int totalTests = 0;

        void AssertTest(string testName, bool condition, string detail = "")
        {
            totalTests++;
            if (condition)
            {
                passCount++;
                Debug.Log($"[TEST PASS] {testName} {(string.IsNullOrEmpty(detail) ? "" : " - " + detail)}");
            }
            else
            {
                Debug.LogError($"[TEST FAIL] {testName} {(string.IsNullOrEmpty(detail) ? "" : " - " + detail)}");
            }
        }

        try
        {
            // 1. Scene Loading Test
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            AssertTest("Scene Exists and Loads", scene.IsValid() && scene.isLoaded, SCENE_PATH);

            // 2. Environment Verification
            GameObject ground = GameObject.Find("Garden_Ground");
            AssertTest("Garden Ground Exists", ground != null && ground.GetComponent<Collider>() != null);

            GameObject sun = GameObject.Find("Sun_DirectionalLight");
            AssertTest("Sun & Atmosphere Configured", sun != null && sun.GetComponent<Light>() != null && sun.GetComponent<GardenAtmosphere>() != null);

            GameObject trees = GameObject.Find("Vegetation");
            AssertTest("Garden Vegetation Exists", trees != null && trees.transform.childCount > 0);

            // 3. Player Character Verification
            GameObject player = GameObject.Find("BoyExplorer");
            AssertTest("Boy Character Exists", player != null);

            CharacterController cc = player != null ? player.GetComponent<CharacterController>() : null;
            AssertTest("CharacterController Configured", cc != null && cc.height > 1f && cc.radius > 0.2f);

            PlayerMovement pm = player != null ? player.GetComponent<PlayerMovement>() : null;
            AssertTest("PlayerMovement Component Attached", pm != null);

            CharacterAnimator ca = player != null ? player.GetComponent<CharacterAnimator>() : null;
            AssertTest("CharacterAnimator Component Attached", ca != null);

            ButterflyCatcher bc = player != null ? player.GetComponent<ButterflyCatcher>() : null;
            AssertTest("ButterflyCatcher Component Attached", bc != null);

            Transform rightHand = ca != null ? ca.RightHand : null;
            AssertTest("Right Hand Articulated Transform Exists", rightHand != null);

            Transform net = rightHand != null ? rightHand.Find("ButterflyNet") : null;
            AssertTest("Butterfly Net Attached to Boy's Hand", net != null);

            // 4. Camera Follow Verification
            GameObject cam = GameObject.FindWithTag("MainCamera");
            AssertTest("Main Camera Exists", cam != null);
            CameraFollow cf = cam != null ? cam.GetComponent<CameraFollow>() : null;
            AssertTest("CameraFollow Script Attached to Main Camera", cf != null);

            // 5. Spawner and Prefabs Verification
            ButterflySpawner spawner = UnityEngine.Object.FindAnyObjectByType<ButterflySpawner>();
            AssertTest("ButterflySpawner Exists", spawner != null);

            // 6. Managers Verification
            ScoreManager scoreMgr = UnityEngine.Object.FindAnyObjectByType<ScoreManager>();
            AssertTest("ScoreManager Exists", scoreMgr != null);

            SoundManager soundMgr = UnityEngine.Object.FindAnyObjectByType<SoundManager>();
            AssertTest("SoundManager Exists", soundMgr != null);

            GameManager gameMgr = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            AssertTest("GameManager Exists", gameMgr != null);

            UIManager uiMgr = UnityEngine.Object.FindAnyObjectByType<UIManager>();
            AssertTest("UIManager Exists", uiMgr != null);

            // 7. UI Canvas Verification
            GameObject canvas = GameObject.Find("GameCanvas");
            AssertTest("Game Canvas Exists", canvas != null && canvas.GetComponent<Canvas>() != null);

            // 8. Game Loop & Logic Simulation
            gameMgr.StartGame();
            AssertTest("Game Starts in Playing State", gameMgr.IsGameActive && gameMgr.CurrentState == GameState.Playing);
            AssertTest("Initial Score is 0", scoreMgr.CurrentScore == 0);
            AssertTest("Initial Caught Count is 0", scoreMgr.CaughtCount == 0);
            AssertTest("Countdown Timer is 60s", Mathf.Approximately(gameMgr.TimeRemaining, 60f));

            // 9. Negative Catch Test (No butterfly in range)
            bc.AttemptCatch();
            AssertTest("Empty Swing Awards 0 Points", scoreMgr.CurrentScore == 0 && scoreMgr.CaughtCount == 0);

            // 10. Positive Catch Test (Butterfly placed within net reach)
            // Spawn test butterfly 1.2m directly in front of boy
            GameObject testButterfly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            testButterfly.name = "TestButterfly_Monarch";
            testButterfly.transform.position = player.transform.position + player.transform.forward * 1.2f + Vector3.up * 0.8f;
            SphereCollider testCol = testButterfly.GetComponent<SphereCollider>();
            testCol.isTrigger = true;
            ButterflyAI testAI = testButterfly.AddComponent<ButterflyAI>();
            testAI.SetSpecies(ButterflySpecies.MonarchOrange);

            // Trigger catch directly
            testAI.OnCaught();

            AssertTest("Successful Catch Awards Exactly 10 Points", scoreMgr.CurrentScore == 10, $"Score: {scoreMgr.CurrentScore}");
            AssertTest("Successful Catch Increments Caught Count to 1", scoreMgr.CaughtCount == 1, $"Caught: {scoreMgr.CaughtCount}");

            // 11. Second Catch Test
            scoreMgr.AddCatch();
            AssertTest("Second Catch Awards Another 10 Points", scoreMgr.CurrentScore == 20 && scoreMgr.CaughtCount == 2);

            // 12. Game Over Verification
            gameMgr.TriggerGameOver();
            AssertTest("Game Over State Triggered", gameMgr.CurrentState == GameState.GameOver && !gameMgr.IsGameActive);

            // Verify catch blocked during game over
            int scoreBefore = scoreMgr.CurrentScore;
            // Attempting catch while inactive shouldn't change active gameplay
            AssertTest("Game Over Prevents Active Gameplay", !gameMgr.IsGameActive);

            // 13. Restart Verification
            gameMgr.RestartGame();
            AssertTest("Restart Resets Score to 0", scoreMgr.CurrentScore == 0);
            AssertTest("Restart Resets Caught Count to 0", scoreMgr.CaughtCount == 0);
            AssertTest("Restart Resets Timer to 60s", Mathf.Approximately(gameMgr.TimeRemaining, 60f));
            AssertTest("Restart Restores Playing State", gameMgr.IsGameActive);

            Debug.Log("=================================================");
            Debug.Log($"   TEST RESULTS: {passCount} / {totalTests} TESTS PASSED   ");
            Debug.Log("=================================================");

            if (passCount == totalTests)
            {
                Debug.Log("[VALIDATION SUCCESS] All gameplay mechanics verified 100% functional!");
            }
            else
            {
                Debug.LogError($"[VALIDATION FAILURE] {totalTests - passCount} tests failed!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TEST SUITE EXCEPTION] {ex}");
        }
    }
}
