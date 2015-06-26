using UnityEngine;

public class GameHud : MonoBehaviour
{
    public GUISkin Skin;
    public bool CheckpointReached { get; set; }

    public void OnGUI()
    {
        GUI.skin = Skin;
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        {
            GUILayout.BeginVertical(Skin.GetStyle("GameHUD"));
            {
                GUILayout.Label(string.Format("Points: {0}", GameManager.Instance.Points), Skin.GetStyle("PointsText"));

                var time = LevelManager.Instance.RunningTime;
                if (!LevelManager.Instance.IsAtLastCheckpoint)
                    GUILayout.Label(string.Format("Time Remaining: {0:00} : {1:00} with Bonus {2}",
                        time.Minutes, Mathf.Max(0, LevelManager.Instance.BonusCutoffSeconds - time.Seconds),
                        LevelManager.Instance.CurrentTimeBonus), Skin.GetStyle("TimeText"));

                if (CheckpointReached)
                {
                    GUILayout.Label("Checkpoint Reached", Skin.GetStyle("CheckpointDisplayText"));
                }

            }
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
    }
}