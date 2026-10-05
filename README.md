# HeartVillage 미니게임 작업 안내

## 호흡 미니게임 담당자 작업 예시

호흡 미니게임 담당자는 아래 폴더만 사용하면 됩니다.

```text
Assets/Features/Breathing/
├─ Scenes
├─ Scripts
├─ Prefabs
├─ Sprites
├─ Animations
├─ Audio
├─ UI
└─ Data
```

### 작업 방법

1. 다음 씬을 엽니다.

```text
Assets/Features/Breathing/Scenes/BreathingMinigame.unity
```

2. 필요한 파일을 종류에 맞게 저장합니다.

```text
호흡 미니게임 코드
→ Breathing/Scripts

버튼, 게이지 등의 UI
→ Breathing/UI

재사용할 게임 오브젝트
→ Breathing/Prefabs

이미지
→ Breathing/Sprites

애니메이션
→ Breathing/Animations

효과음과 배경음
→ Breathing/Audio

설정값이나 ScriptableObject
→ Breathing/Data
```

예를 들면 다음과 같이 저장합니다.

```text
Assets/Features/Breathing/Scripts/BreathingGameManager.cs
Assets/Features/Breathing/Scripts/BreathingInputController.cs
Assets/Features/Breathing/UI/BreathingGauge.prefab
Assets/Features/Breathing/Sprites/BreathingCircle.png
Assets/Features/Breathing/Audio/BreathingBGM.wav
```

호흡 미니게임에서만 사용하는 파일은 전부 `Assets/Features/Breathing` 폴더 안에 넣으면 됩니다.

여러 미니게임에서 같이 사용해야 하는 파일만 다음 폴더에 넣습니다.

```text
Assets/Shared
```

`Core`, `Exercise`, `Diet` 등 다른 담당자의 폴더와 씬은 수정하지 않습니다.

작업할 브랜치는 다음과 같습니다.

```text
feature_breathing
```

정리하면 `feature_breathing` 브랜치에서 `BreathingMinigame.unity`를 열고, 필요한 파일은 모두 `Assets/Features/Breathing` 아래의 알맞은 폴더에 저장하면 됩니다.

