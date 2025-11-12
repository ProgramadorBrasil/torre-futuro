using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRPG.Systems
{
    /// <summary>
    /// Advanced NPC Instructor system with animated avatar, contextual dialogue, and tutorial management
    /// Provides interactive guidance, quest tracking, and dynamic responses to player actions
    /// </summary>
    public class NPCInstructor : MonoBehaviour
{
    #region Dialogue System
    [System.Serializable]
    public class DialogueEntry
    {
        public string id;
        public string speakerName;
        [TextArea(3, 6)]
        public string text;
        public AudioClip voiceClip;
        public float displayDuration;
        public bool requiresResponse;
        public List<string> responses;
    }

    public enum TutorialStage
    {
        Welcome,
        Movement,
        Weapons,
        Combat,
        Planting,
        Upgrades,
        Advanced,
        Completed
    }
    #endregion

    [Header("NPC Configuration")]
    [SerializeField] private string instructorName = "Commander Aurora";
    [SerializeField] private GameObject avatarModel;
    [SerializeField] private Animator avatarAnimator;
    [SerializeField] private Transform lookAtTarget; // Camera or player

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Transform playerTransform;

    [Header("Dialogue System")]
    [SerializeField] private List<DialogueEntry> dialogueDatabase = new List<DialogueEntry>();
    [SerializeField] private float defaultDialogueDuration = 5f;
    [SerializeField] private float typingSpeed = 0.05f; // Seconds per character
    [SerializeField] private bool autoAdvanceDialogue = true;

    [Header("Tutorial System")]
    [SerializeField] private TutorialStage currentStage = TutorialStage.Welcome;
    [SerializeField] private bool tutorialCompleted = false;
    [SerializeField] private bool skipTutorial = false;

    [Header("Animation")]
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string talkAnimation = "Talk";
    [SerializeField] private string pointAnimation = "Point";
    [SerializeField] private string waveAnimation = "Wave";
    [SerializeField] private bool useHeadTracking = true;
    [SerializeField] private float headTrackingSpeed = 2f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject questMarkerPrefab;
    [SerializeField] private ParticleSystem dialogueParticles;
    [SerializeField] private Light characterLight;
    [SerializeField] private Color normalLightColor = Color.white;
    [SerializeField] private Color questLightColor = Color.yellow;

    [Header("Audio")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioClip greetingSound;
    [SerializeField] private AudioClip questCompleteSound;
    [SerializeField] private AudioClip typingSound;

    [Header("Quest System")]
    [SerializeField] private bool hasActiveQuest = false;
    [SerializeField] private string currentQuestDescription = "";
    [SerializeField] private int questRewardCredits = 0;
    [SerializeField] private int questRewardXP = 0;

    // Private variables
    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private Coroutine currentDialogue = null;
    private GameObject questMarker;
    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
    private Dictionary<string, DialogueEntry> dialogueDict = new Dictionary<string, DialogueEntry>();
    private Transform headBone;

    // References
    private GameplayUI gameplayUI;
    private RewardSystem rewardSystem;
    private UpgradeSystem upgradeSystem;

    // Events
    public delegate void DialogueEvent(string dialogueId);
    public delegate void TutorialEvent(TutorialStage stage);
    public event DialogueEvent OnDialogueStarted;
    public event DialogueEvent OnDialogueEnded;
    public event TutorialEvent OnTutorialStageCompleted;
    public event System.Action OnQuestCompleted;

    #region Initialization

    private void Awake()
    {
        if (avatarAnimator == null && avatarModel != null)
        {
            avatarAnimator = avatarModel.GetComponent<Animator>();
        }

        if (voiceAudioSource == null)
        {
            voiceAudioSource = gameObject.AddComponent<AudioSource>();
            voiceAudioSource.spatialBlend = 1f;
            voiceAudioSource.maxDistance = 20f;
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        FindReferences();
        InitializeDialogueSystem();

        // Find head bone for look-at
        if (avatarModel != null && useHeadTracking)
        {
            headBone = FindHeadBone(avatarModel.transform);
        }
    }

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Create quest marker
        if (questMarkerPrefab != null)
        {
            questMarker = Instantiate(questMarkerPrefab, transform.position + Vector3.up * 3f, Quaternion.identity, transform);
            questMarker.SetActive(false);
        }

        // Start with welcome dialogue if not skipping tutorial
        if (!skipTutorial && !tutorialCompleted)
        {
            StartCoroutine(DelayedWelcome());
        }
    }

    private void FindReferences()
    {
        if (gameplayUI == null)
            gameplayUI = FindObjectOfType<GameplayUI>();

        if (rewardSystem == null)
            rewardSystem = FindObjectOfType<RewardSystem>();

        if (upgradeSystem == null)
            upgradeSystem = FindObjectOfType<UpgradeSystem>();
    }

    private void InitializeDialogueSystem()
    {
        if (dialogueDatabase.Count == 0)
        {
            CreateDefaultDialogues();
        }

        dialogueDict.Clear();
        foreach (var dialogue in dialogueDatabase)
        {
            if (!dialogueDict.ContainsKey(dialogue.id))
            {
                dialogueDict.Add(dialogue.id, dialogue);
            }
        }
    }

    private void CreateDefaultDialogues()
    {
        // Welcome dialogue
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "welcome",
            speakerName = instructorName,
            text = "Welcome, pilot! I'm Commander Aurora, your flight instructor. I'll guide you through your training. Are you ready to begin?",
            displayDuration = 8f,
            requiresResponse = false
        });

        // Movement tutorial
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "movement_tutorial",
            speakerName = instructorName,
            text = "Let's start with the basics. Use WASD keys to control your ship's movement. Mouse to look around. Press SHIFT to boost!",
            displayDuration = 10f,
            requiresResponse = false
        });

        // Weapons tutorial
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "weapons_tutorial",
            speakerName = instructorName,
            text = "Now for weapons! Left-click or SPACE to fire. Use 1, 2, 3 to switch weapons. Press R to reload. Show me what you've got!",
            displayDuration = 10f,
            requiresResponse = false
        });

        // Planting tutorial
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "planting_tutorial",
            speakerName = instructorName,
            text = "You can grow resources! Press P to enter planting mode. Click to plant seeds. Press H to harvest when they're grown. Smart investment!",
            displayDuration = 10f,
            requiresResponse = false
        });

        // Upgrades tutorial
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "upgrades_tutorial",
            speakerName = instructorName,
            text = "Visit the upgrade station to enhance your ship and weapons. Earn credits by completing missions and harvesting plants. Spend wisely!",
            displayDuration = 10f,
            requiresResponse = false
        });

        // Quest available
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "quest_available",
            speakerName = instructorName,
            text = "I have a mission for you, pilot. Interested in earning some extra credits? Press E to accept.",
            displayDuration = 6f,
            requiresResponse = true,
            responses = new List<string> { "Accept Quest", "Maybe Later" }
        });

        // Quest completed
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "quest_complete",
            speakerName = instructorName,
            text = "Excellent work, pilot! Mission accomplished. Here's your reward. Keep up the good work!",
            displayDuration = 6f,
            requiresResponse = false
        });

        // Training complete
        dialogueDatabase.Add(new DialogueEntry
        {
            id = "training_complete",
            speakerName = instructorName,
            text = "Your training is complete! You're ready for real combat. Good luck out there, pilot. Make me proud!",
            displayDuration = 7f,
            requiresResponse = false
        });
    }

    private Transform FindHeadBone(Transform root)
    {
        // Try to find head bone by common names
        string[] headBoneNames = { "Head", "head", "Head_Jnt", "neck" };

        foreach (string boneName in headBoneNames)
        {
            Transform bone = FindChildRecursive(root, boneName);
            if (bone != null) return bone;
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name.Contains(name))
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }

        return null;
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        CheckPlayerProximity();
        HandleInteraction();
        UpdateLookAt();
        UpdateQuestMarker();
    }

    private void LateUpdate()
    {
        // Head tracking in LateUpdate for smoother animation
        if (useHeadTracking && headBone != null && lookAtTarget != null)
        {
            Vector3 direction = lookAtTarget.position - headBone.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, Time.deltaTime * headTrackingSpeed);
        }
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        // Player entered range
        if (playerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        // Player left range
        else if (!playerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }

    private void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void UpdateLookAt()
    {
        // Update look target (prefer player when in range)
        if (playerInRange && playerTransform != null)
        {
            lookAtTarget = playerTransform;
        }
        else if (Camera.main != null)
        {
            lookAtTarget = Camera.main.transform;
        }

        // Make NPC face player when in conversation
        if (isDialogueActive && playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }
    }

    private void UpdateQuestMarker()
    {
        if (questMarker != null)
        {
            questMarker.SetActive(hasActiveQuest);

            if (hasActiveQuest)
            {
                // Rotate quest marker
                questMarker.transform.Rotate(Vector3.up, 50f * Time.deltaTime);
            }
        }

        // Update character light color
        if (characterLight != null)
        {
            characterLight.color = hasActiveQuest ? questLightColor : normalLightColor;
        }
    }

    #endregion

    #region Interaction

    private void OnPlayerEnterRange()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        // Play greeting animation
        PlayAnimation(waveAnimation);
        PlaySound(greetingSound);

        Debug.Log($"Press {interactKey} to talk to {instructorName}");
    }

    private void OnPlayerExitRange()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        PlayAnimation(idleAnimation);
    }

    public void Interact()
    {
        if (isDialogueActive) return;

        // Determine which dialogue to show
        if (hasActiveQuest)
        {
            StartDialogue("quest_available");
        }
        else if (!tutorialCompleted)
        {
            AdvanceTutorial();
        }
        else
        {
            StartDialogue("welcome");
        }
    }

    #endregion

    #region Dialogue System

    public void StartDialogue(string dialogueId)
    {
        if (!dialogueDict.ContainsKey(dialogueId))
        {
            Debug.LogWarning($"Dialogue '{dialogueId}' not found!");
            return;
        }

        DialogueEntry dialogue = dialogueDict[dialogueId];

        if (currentDialogue != null)
        {
            StopCoroutine(currentDialogue);
        }

        currentDialogue = StartCoroutine(PlayDialogue(dialogue));
    }

    private IEnumerator PlayDialogue(DialogueEntry dialogue)
    {
        isDialogueActive = true;
        OnDialogueStarted?.Invoke(dialogue.id);

        // Play talking animation
        PlayAnimation(talkAnimation);

        // Play voice clip if available
        if (dialogue.voiceClip != null && voiceAudioSource != null)
        {
            voiceAudioSource.PlayOneShot(dialogue.voiceClip);
        }

        // Show dialogue in UI
        if (gameplayUI != null)
        {
            gameplayUI.ShowDialogue(dialogue.speakerName, dialogue.text);
        }
        else
        {
            Debug.Log($"<b>{dialogue.speakerName}:</b> {dialogue.text}");
        }

        // Activate dialogue particles
        if (dialogueParticles != null)
        {
            dialogueParticles.Play();
        }

        // Wait for dialogue duration
        float duration = dialogue.displayDuration > 0 ? dialogue.displayDuration : defaultDialogueDuration;
        yield return new WaitForSeconds(duration);

        // Hide dialogue
        if (gameplayUI != null)
        {
            gameplayUI.HideDialogue();
        }

        // Stop particles
        if (dialogueParticles != null)
        {
            dialogueParticles.Stop();
        }

        // Return to idle
        PlayAnimation(idleAnimation);

        isDialogueActive = false;
        OnDialogueEnded?.Invoke(dialogue.id);

        currentDialogue = null;
    }

    public void QueueDialogue(string dialogueId)
    {
        if (dialogueDict.ContainsKey(dialogueId))
        {
            dialogueQueue.Enqueue(dialogueDict[dialogueId]);
        }
    }

    private IEnumerator ProcessDialogueQueue()
    {
        while (dialogueQueue.Count > 0)
        {
            DialogueEntry dialogue = dialogueQueue.Dequeue();
            yield return StartCoroutine(PlayDialogue(dialogue));
            yield return new WaitForSeconds(1f);
        }
    }

    #endregion

    #region Tutorial System

    private IEnumerator DelayedWelcome()
    {
        yield return new WaitForSeconds(2f);
        StartDialogue("welcome");
    }

    public void AdvanceTutorial()
    {
        switch (currentStage)
        {
            case TutorialStage.Welcome:
                currentStage = TutorialStage.Movement;
                StartDialogue("movement_tutorial");
                break;

            case TutorialStage.Movement:
                currentStage = TutorialStage.Weapons;
                StartDialogue("weapons_tutorial");
                break;

            case TutorialStage.Weapons:
                currentStage = TutorialStage.Planting;
                StartDialogue("planting_tutorial");
                break;

            case TutorialStage.Planting:
                currentStage = TutorialStage.Upgrades;
                StartDialogue("upgrades_tutorial");
                break;

            case TutorialStage.Upgrades:
                currentStage = TutorialStage.Completed;
                StartDialogue("training_complete");
                CompleteTutorial();
                break;
        }

        OnTutorialStageCompleted?.Invoke(currentStage);
    }

    private void CompleteTutorial()
    {
        tutorialCompleted = true;

        // Grant completion reward
        if (rewardSystem != null)
        {
            rewardSystem.AddCredits(500);
            rewardSystem.AddXP(200);
        }

        Debug.Log("Tutorial completed! Earned 500 credits and 200 XP");
    }

    #endregion

    #region Quest System

    public void OfferQuest(string description, int creditsReward, int xpReward)
    {
        hasActiveQuest = true;
        currentQuestDescription = description;
        questRewardCredits = creditsReward;
        questRewardXP = xpReward;

        Debug.Log($"Quest offered: {description}");
    }

    public void CompleteQuest()
    {
        if (!hasActiveQuest) return;

        hasActiveQuest = false;

        // Grant rewards
        if (rewardSystem != null)
        {
            rewardSystem.AddCredits(questRewardCredits);
            rewardSystem.AddXP(questRewardXP);
        }

        StartDialogue("quest_complete");
        PlaySound(questCompleteSound);

        OnQuestCompleted?.Invoke();

        Debug.Log($"Quest completed! Earned {questRewardCredits} credits and {questRewardXP} XP");
    }

    #endregion

    #region Animation & Audio

    private void PlayAnimation(string animationName)
    {
        if (avatarAnimator == null || string.IsNullOrEmpty(animationName)) return;

        avatarAnimator.CrossFade(animationName, 0.2f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && voiceAudioSource != null)
        {
            voiceAudioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Public Methods

    public bool IsDialogueActive() => isDialogueActive;
    public bool IsTutorialCompleted() => tutorialCompleted;
    public TutorialStage GetCurrentTutorialStage() => currentStage;
    public bool HasActiveQuest() => hasActiveQuest;
    public string GetQuestDescription() => currentQuestDescription;

    public void SetTutorialCompleted(bool completed)
    {
        tutorialCompleted = completed;
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw look direction
        if (lookAtTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, lookAtTarget.position);
        }
    }

    #endregion
}
}
