 using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }

    [Serializable]
    public class TokenType
    {
        public string name;
        public int count;
        public Image iconUI;
        public TextMeshProUGUI countText;
        public int maxStorage = 5;
    }

    public TokenType feedTokens;
    public TokenType playTokens;
    public TokenType sleepTokens;
    public GameObject tokenFeedbackPrefab;
    public Transform feedbackParent;
    public event Action TokensChanged;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        UpdateTokenUI();
    }

    public bool UseToken(string tokenType)
    {
        var t = GetTokenByType(tokenType);
        if (t == null || t.count <= 0)
        {
            SFXManager.Instance?.PlayError();
            return false;
        }
        t.count--;
        UpdateTokenUI();
        SFXManager.Instance?.PlayTokenSpent();
        return true;
    }

    public void AddTokens(string tokenType, int amount)
    {
        var t = GetTokenByType(tokenType);
        if (t == null || amount <= 0) return;
        t.count = Mathf.Min(t.count + amount, t.maxStorage);
        UpdateTokenUI();
        ShowTokenFeedback(tokenType, amount);
        SFXManager.Instance?.PlayTokenEarned();
    }

    public bool HasToken(string tokenType) => GetCount(tokenType) > 0;
    public int GetCount(string tokenType) => GetTokenByType(tokenType)?.count ?? 0;

    TokenType GetTokenByType(string tokenType)
    {
        switch (tokenType.ToLowerInvariant())
        {
            case "feed": return feedTokens;
            case "play": return playTokens;
            case "sleep": return sleepTokens;
            default: return null;
        }
    }

    void UpdateTokenUI()
    {
        UpdateSingle(feedTokens);
        UpdateSingle(playTokens);
        UpdateSingle(sleepTokens);
        TokensChanged?.Invoke();
    }

    void UpdateSingle(TokenType t)
    {
        if (t != null && t.countText)
            t.countText.text = $"{t.count}/{t.maxStorage}";
    }

    void ShowTokenFeedback(string tokenType, int amount)
    {
        if (!tokenFeedbackPrefab || !feedbackParent) return;
        var go = Instantiate(tokenFeedbackPrefab, feedbackParent);
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        if (text) text.text = $"+{amount} {tokenType}";

        var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
        LeanTween.moveY(go, go.transform.position.y + 100f, 1.2f);
        LeanTween.alphaCanvas(cg, 0f, 1.2f).setOnComplete(() => Destroy(go));
    }
}