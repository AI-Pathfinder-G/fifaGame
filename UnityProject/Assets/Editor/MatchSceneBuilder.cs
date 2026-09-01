using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SoccerGame.Core;
using SoccerGame.Player;
using SoccerGame.Ball;
using SoccerGame.Camera;
using SoccerGame.Data;
using SoccerGame.Match;
using SoccerGame.UI;
using SoccerGame.Audio;
using SoccerGame.AI;
using System.Collections.Generic;

public static class MatchSceneBuilder
{
    public static void BuildMatchScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateField();
        CreateGoals();
        CreateBall();
        CreateCamera();
        CreatePlayers();
        CreateGameManager();
        CreateMatchSystems();
        CreateUI();
        CreateAudio();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Match.unity");
        Debug.Log("[MatchSceneBuilder] Match scene built successfully.");
    }

    private static GameObject CreateField()
    {
        GameObject field = GameObject.CreatePrimitive(PrimitiveType.Plane);
        field.name = "Field";
        field.transform.position = Vector3.zero;
        field.transform.localScale = new Vector3(10.5f, 1f, 6.8f);

        Renderer r = field.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = new Color(0.18f, 0.55f, 0.22f);
        r.material.name = "Grass";

        CreateFieldLines();
        return field;
    }

    private static void CreateFieldLines()
    {
        GameObject linesParent = new GameObject("FieldLines");

        CreateLineObject("CenterLine", linesParent.transform, new Vector3(0f, 0.01f, 0f), new Vector3(68f, 0.02f, 0.5f), Color.white);
        CreateLineObject("LeftTouchline", linesParent.transform, new Vector3(-52.5f, 0.01f, 0f), new Vector3(0.5f, 0.02f, 68f), Color.white);
        CreateLineObject("RightTouchline", linesParent.transform, new Vector3(52.5f, 0.01f, 0f), new Vector3(0.5f, 0.02f, 68f), Color.white);
        CreateLineObject("GoalLineHome", linesParent.transform, new Vector3(0f, 0.01f, -34f), new Vector3(105f, 0.02f, 0.5f), Color.white);
        CreateLineObject("GoalLineAway", linesParent.transform, new Vector3(0f, 0.01f, 34f), new Vector3(105f, 0.02f, 0.5f), Color.white);

        CreateLineObject("PenaltyBoxHome1", linesParent.transform, new Vector3(-16.5f, 0.01f, -34f), new Vector3(0.3f, 0.02f, 40.3f), Color.white);
        CreateLineObject("PenaltyBoxHome2", linesParent.transform, new Vector3(16.5f, 0.01f, -34f), new Vector3(0.3f, 0.02f, 40.3f), Color.white);
        CreateLineObject("PenaltyBoxHome3", linesParent.transform, new Vector3(0f, 0.01f, -17.5f), new Vector3(33.6f, 0.02f, 0.3f), Color.white);

        CreateLineObject("PenaltyBoxAway1", linesParent.transform, new Vector3(-16.5f, 0.01f, 34f), new Vector3(0.3f, 0.02f, 40.3f), Color.white);
        CreateLineObject("PenaltyBoxAway2", linesParent.transform, new Vector3(16.5f, 0.01f, 34f), new Vector3(0.3f, 0.02f, 40.3f), Color.white);
        CreateLineObject("PenaltyBoxAway3", linesParent.transform, new Vector3(0f, 0.01f, 17.5f), new Vector3(33.6f, 0.02f, 0.3f), Color.white);

        CreateCircle("CenterCircle", linesParent.transform, new Vector3(0f, 0.01f, 0f), 9.15f, Color.white);
    }

    private static void CreateLineObject(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = name;
        line.transform.SetParent(parent);
        line.transform.position = pos;
        line.transform.localScale = scale;
        Renderer r = line.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = color;
    }

    private static void CreateCircle(string name, Transform parent, Vector3 center, float radius, Color color)
    {
        GameObject circleParent = new GameObject(name);
        circleParent.transform.SetParent(parent);
        int segments = 48;
        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dot.name = $"Circle_{i}";
            dot.transform.SetParent(circleParent.transform);
            dot.transform.position = pos;
            dot.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
            Renderer r = dot.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.color = color;
        }
    }

    private static void CreateGoals()
    {
        CreateGoal("GoalHome", new Vector3(0f, 1.22f, -34f), TeamSide.Home);
        CreateGoal("GoalAway", new Vector3(0f, 1.22f, 34f), TeamSide.Away);
    }

    private static void CreateGoal(string name, Vector3 pos, TeamSide defendingTeam)
    {
        GameObject goal = new GameObject(name);
        goal.transform.position = pos;

        float goalWidth = FieldDimensions.GoalWidth;
        float goalHeight = FieldDimensions.GoalHeight;

        CreatePost(goal.transform, "PostLeft", new Vector3(-goalWidth * 0.5f, 0f, 0f), goalHeight);
        CreatePost(goal.transform, "PostRight", new Vector3(goalWidth * 0.5f, 0f, 0f), goalHeight);
        CreatePost(goal.transform, "Crossbar", new Vector3(0f, goalHeight, 0f), goalWidth, true);

        GameObject trigger = new GameObject("GoalTrigger");
        trigger.transform.SetParent(goal.transform);
        trigger.transform.localPosition = Vector3.zero;
        trigger.tag = "GoalTrigger";
        BoxCollider bc = trigger.AddComponent<BoxCollider>();
        bc.size = new Vector3(goalWidth, goalHeight, 0.5f);
        bc.isTrigger = true;
        GoalTrigger gt = trigger.AddComponent<GoalTrigger>();
        gt.SetDefendingTeam(defendingTeam);

        goal.tag = "GoalPost";
    }

    private static void CreatePost(Transform parent, string name, Vector3 localPos, float length, bool horizontal = false)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = name;
        post.transform.SetParent(parent);
        post.transform.localPosition = localPos;
        if (horizontal)
        {
            post.transform.Rotate(0f, 0f, 90f);
            post.transform.localScale = new Vector3(0.1f, length * 0.5f, 0.1f);
        }
        else
        {
            post.transform.localScale = new Vector3(0.1f, length * 0.5f, 0.1f);
        }
        Renderer r = post.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = Color.white;
    }

    private static void CreateBall()
    {
        GameObject ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballObj.name = "Ball";
        ballObj.transform.position = new Vector3(0f, 0.11f, 0f);
        ballObj.transform.localScale = Vector3.one * 0.22f;
        ballObj.tag = "Ball";

        Rigidbody rb = ballObj.AddComponent<Rigidbody>();
        rb.mass = 0.43f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        SphereCollider sc = ballObj.GetComponent<SphereCollider>();
        sc.radius = 0.11f;

        Renderer r = ballObj.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = Color.white;

        BallEntity ball = ballObj.AddComponent<BallEntity>();
        BallPhysics bp = ballObj.AddComponent<BallPhysics>();
        BallCollisionHandler bch = ballObj.AddComponent<BallCollisionHandler>();
    }

    private static void CreateCamera()
    {
        GameObject camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.4f, 0.6f);
        cam.fieldOfView = 40f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;

        AudioListener listener = camObj.AddComponent<AudioListener>();

        BroadcastCameraController camCtrl = camObj.AddComponent<BroadcastCameraController>();
        camObj.transform.position = new Vector3(0f, 25f, -40f);
        camObj.transform.LookAt(Vector3.zero);
    }

    private static void CreatePlayers()
    {
        TeamData homeData = CreateTeamData("Red FC", "RFC", new Color(0.8f, 0.1f, 0.1f), Color.white);
        TeamData awayData = CreateTeamData("Blue United", "BLU", new Color(0.1f, 0.2f, 0.8f), Color.white);
        FormationData formation = CreateFormationData();

        GameObject homeParent = new GameObject("HomeTeam");
        GameObject awayParent = new GameObject("AwayTeam");

        for (int i = 0; i < 11; i++)
        {
            FormationSlot slot = formation.Slots[i];
            Vector3 homePos = formation.GetWorldPosition(i, FieldDimensions.FieldLength, FieldDimensions.FieldWidth, TeamSide.Home);
            Vector3 awayPos = formation.GetWorldPosition(i, FieldDimensions.FieldLength, FieldDimensions.FieldWidth, TeamSide.Away);

            PositionRole role = slot.Role;
            bool isGK = role == PositionRole.GK;

            CreatePlayer($"Home_{i}", homePos, homeParent.transform, TeamSide.Home, homeData, i, isGK, role);
            CreatePlayer($"Away_{i}", awayPos, awayParent.transform, TeamSide.Away, awayData, i, isGK, role);
        }
    }

    private static void CreatePlayer(string name, Vector3 pos, Transform parent, TeamSide team, TeamData data, int fieldNumber, bool isGK, PositionRole role)
    {
        GameObject playerObj = new GameObject(name);
        playerObj.transform.SetParent(parent);
        playerObj.transform.position = pos;
        playerObj.tag = "Player";

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(playerObj.transform);
        body.transform.localPosition = Vector3.up * 0.9f;
        body.transform.localScale = new Vector3(0.4f, 0.9f, 0.4f);

        Renderer r = body.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = team == TeamSide.Home ? data.PrimaryColor : data.PrimaryColor;

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(playerObj.transform);
        head.transform.localPosition = Vector3.up * 1.8f;
        head.transform.localScale = Vector3.one * 0.25f;

        Renderer hr = head.GetComponent<Renderer>();
        hr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hr.material.color = new Color(0.85f, 0.65f, 0.5f);

        CapsuleCollider col = playerObj.AddComponent<CapsuleCollider>();
        col.height = 1.8f;
        col.radius = 0.3f;
        col.center = Vector3.up * 0.9f;

        Rigidbody rb = playerObj.AddComponent<Rigidbody>();
        rb.mass = 70f;
        rb.linearDamping = 5f;
        rb.angularDamping = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        PlayerEntity entity = playerObj.AddComponent<PlayerEntity>();
        entity.Data = CreatePlayerData(name, role, isGK);
        entity.Team = team;
        entity.Role = role;
        entity.FieldNumber = fieldNumber;
        entity.FormationPosition = pos;

        if (fieldNumber == 0 && team == TeamSide.Home)
        {
            PlayerController controller = playerObj.AddComponent<PlayerController>();
            GameObject cam = GameObject.Find("MainCamera");
            if (cam)
            {
                SerializedObject cso = new SerializedObject(controller);
                cso.FindProperty("cameraTransform").objectReferenceValue = cam.transform;
                cso.FindProperty("player").objectReferenceValue = entity;
                cso.ApplyModifiedProperties();
            }
            entity.IsUserControlled = true;
        }

        PlayerAI ai = playerObj.AddComponent<PlayerAI>();
        PlayerAnimator anim = playerObj.AddComponent<PlayerAnimator>();
    }

    private static void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        SoccerGame.Core.GameManager gm = gmObj.AddComponent<SoccerGame.Core.GameManager>();
    }

    private static void CreateMatchSystems()
    {
        GameObject matchObj = new GameObject("MatchSystem");
        MatchManager match = matchObj.AddComponent<MatchManager>();

        SetPieceController spc = matchObj.AddComponent<SetPieceController>();
        BallEntity ball = GameObject.Find("Ball")?.GetComponent<BallEntity>();
        if (ball != null)
        {
            SerializedObject spcSo = new SerializedObject(spc);
            spcSo.FindProperty("ball").objectReferenceValue = ball;
            spcSo.ApplyModifiedProperties();
        }

        SerializedObject so = new SerializedObject(match);
        so.FindProperty("setPieceController").objectReferenceValue = spc;
        so.ApplyModifiedProperties();

        RefereeSystem refSystem = matchObj.AddComponent<RefereeSystem>();
        SerializedObject rso = new SerializedObject(refSystem);
        rso.FindProperty("match").objectReferenceValue = match;
        if (ball != null)
        {
            rso.FindProperty("ball").objectReferenceValue = ball;
        }
        rso.ApplyModifiedProperties();

        AIManager aiMgr = matchObj.AddComponent<AIManager>();
    }

    private static void CreateUI()
    {
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject scorePanel = CreateUIPanel("Scoreboard", canvasObj.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(380, -10), new Vector2(0, 1));
        ScoreboardUI scoreboard = scorePanel.AddComponent<ScoreboardUI>();

        CreateUIText("HomeScore", scorePanel.transform, new Vector2(-120, -25), "0", 32);
        CreateUIText("AwayScore", scorePanel.transform, new Vector2(120, -25), "0", 32);
        CreateUIText("Clock", scorePanel.transform, new Vector2(0, -25), "00:00", 24);

        UIManager uiMgr = canvasObj.AddComponent<UIManager>();
    }

    private static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panel.transform.SetParent(parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(380, 60);
        UnityEngine.UI.Image img = panel.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, 0.6f);
        return panel;
    }

    private static void CreateUIText(string name, Transform parent, Vector2 pos, string text, int fontSize)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform));
        textObj.transform.SetParent(parent);
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(120, 50);
        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.color = Color.white;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static void CreateAudio()
    {
        GameObject audioObj = new GameObject("AudioManager");
        AudioManager am = audioObj.AddComponent<AudioManager>();
    }

    private static TeamData CreateTeamData(string name, string shortName, Color primary, Color secondary)
    {
        TeamData asset = ScriptableObject.CreateInstance<TeamData>();
        asset.TeamName = name;
        asset.ShortName = shortName;
        asset.PrimaryColor = primary;
        asset.SecondaryColor = secondary;
        asset.StartingXI = new PlayerData[11];
        asset.Substitutes = new PlayerData[7];
        for (int i = 0; i < 11; i++)
            asset.StartingXI[i] = CreatePlayerData($"{name} P{i + 1}", i == 0 ? PositionRole.GK : PositionRole.CM, i == 0);
        asset.OverallRating = 75;
        return asset;
    }

    private static PlayerData CreatePlayerData(string name, PositionRole role, bool isGK)
    {
        PlayerData asset = ScriptableObject.CreateInstance<PlayerData>();
        asset.PlayerName = name;
        asset.FieldNumber = Random.Range(1, 99);
        asset.PreferredRole = role;
        asset.IsGoalkeeper = isGK;
        asset.Stats = new PlayerStats
        {
            Pace = Random.Range(60, 90),
            Shooting = Random.Range(60, 90),
            Passing = Random.Range(60, 90),
            Dribbling = Random.Range(60, 90),
            Defending = Random.Range(60, 90),
            Stamina = Random.Range(70, 95),
            Strength = Random.Range(60, 85),
            Finishing = Random.Range(55, 88),
            BallControl = Random.Range(60, 88),
            Technique = Random.Range(60, 88),
            Tackling = isGK ? 30 : Random.Range(55, 85),
            Vision = Random.Range(55, 88),
            Composure = Random.Range(55, 85),
            GKReflexes = isGK ? Random.Range(75, 92) : 30,
            GKDiving = isGK ? Random.Range(70, 90) : 30,
            GKHandling = isGK ? Random.Range(70, 90) : 30,
            GKKicking = isGK ? Random.Range(70, 90) : 30,
            GKPositioning = isGK ? Random.Range(72, 92) : 30
        };
        return asset;
    }

    private static FormationData CreateFormationData()
    {
        FormationData asset = ScriptableObject.CreateInstance<FormationData>();
        asset.Name = "4-4-2";
        asset.Slots = new FormationSlot[11];

        asset.Slots[0] = new FormationSlot { Role = PositionRole.GK, BasePosition = new Vector2(0.05f, 0.5f), AttackBias = 0f, DefenseBias = 0f };

        asset.Slots[1] = new FormationSlot { Role = PositionRole.LB, BasePosition = new Vector2(0.2f, 0.15f), AttackBias = 0.3f, DefenseBias = 0.2f };
        asset.Slots[2] = new FormationSlot { Role = PositionRole.LCB, BasePosition = new Vector2(0.15f, 0.38f), AttackBias = 0.2f, DefenseBias = 0.3f };
        asset.Slots[3] = new FormationSlot { Role = PositionRole.RCB, BasePosition = new Vector2(0.15f, 0.62f), AttackBias = 0.2f, DefenseBias = 0.3f };
        asset.Slots[4] = new FormationSlot { Role = PositionRole.RB, BasePosition = new Vector2(0.2f, 0.85f), AttackBias = 0.3f, DefenseBias = 0.2f };

        asset.Slots[5] = new FormationSlot { Role = PositionRole.LM, BasePosition = new Vector2(0.4f, 0.15f), AttackBias = 0.5f, DefenseBias = 0.2f };
        asset.Slots[6] = new FormationSlot { Role = PositionRole.LCM, BasePosition = new Vector2(0.38f, 0.38f), AttackBias = 0.5f, DefenseBias = 0.3f };
        asset.Slots[7] = new FormationSlot { Role = PositionRole.RCM, BasePosition = new Vector2(0.38f, 0.62f), AttackBias = 0.5f, DefenseBias = 0.3f };
        asset.Slots[8] = new FormationSlot { Role = PositionRole.RM, BasePosition = new Vector2(0.4f, 0.85f), AttackBias = 0.5f, DefenseBias = 0.2f };

        asset.Slots[9] = new FormationSlot { Role = PositionRole.ST, BasePosition = new Vector2(0.65f, 0.38f), AttackBias = 0.7f, DefenseBias = 0.1f };
        asset.Slots[10] = new FormationSlot { Role = PositionRole.ST, BasePosition = new Vector2(0.65f, 0.62f), AttackBias = 0.7f, DefenseBias = 0.1f };

        return asset;
    }
}