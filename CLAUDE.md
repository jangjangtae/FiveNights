# FiveNights (가제: 다섯 번의 밤)

**한 줄 요약:** 탐사 기자가 5일 밤 동안 비밀 연구소에 숨어들어 증거를 빼내는 탑다운 2D 공포 잠입 게임. 경비는 밤마다 플레이어의 침입 패턴을 기억하고 대응을 바꾼다.

- 기획서: [docs/GDD.md](docs/GDD.md). 기획 판단이 필요하면 먼저 이 문서를 보고, 문서에 없는 내용은 임의로 정하지 말고 사용자에게 물어볼 것.
- 목적: 1인 개발 포트폴리오 (게임 AI 직군 + 일반 게임 개발자). 웹(WebGL)으로 공개 (itch.io나 개인 페이지).

## 현재 단계: 0단계 프로토타입

통과 기준: **들키고 도망치는 게 재미있는가.**

- [x] 테스트용 방 하나 (`Scenes/Prototype_Room`: 사각형 블록 벽·엄폐물, 벽 충돌, URP 2D 라이트와 그림자. Tilemap 전환은 1단계에서)
- [x] 플레이어 이동: 걷기 / 웅크리기(C 토글) / 달리기(Shift) (Input System, Rigidbody2D)
- [x] 발소리 반경 (속도별 반경, 청록 원으로 표시. 경비 청각 연결은 아직)
- [x] 경비 1명: 순찰 경로 따라 이동 (`PatrolRoute_01`, 문을 지나 두 방을 도는 4지점)
- [x] 경비 시야: 부채꼴 + 레이캐스트 가림 판정, 손전등 라이트 + 벽에 잘리는 시야 메쉬
- [x] 의심 게이지 → 발각 → 추격 → 놓치면 수색 → 복귀 (FsmGuardBrain). 소리로는 게이지 0.8까지만 (의심만, 발각은 눈으로)
- [x] 2D 경로 탐색: 패키지 없이 직접 만든 격자 A* (`Core/Navigation`, `INavQuery` 뒤에 있어 NavMeshPlus 등으로 교체 가능)
- [x] 잡힘 처리: 목숨 -1, 플레이어·경비 시작 위치로 리셋 (목숨 0이면 로그만 남기고 다시 채움. 밤 실패 처리는 2단계)
- [x] 디버그 표시: 시야, 소리 반경, 의심 게이지(머리 위 막대), 상태·목숨(왼쪽 위, F1)
- [ ] 수치 튜닝: 직접 플레이하며 발각 속도·추격 속도 조정 (`GuardSettings`, `PlayerMovementSettings`)
- [ ] WebGL 빌드 테스트 (브라우저 실행, 용량, 프레임)
- [ ] 모델 추론 테스트: 더미 정책 모델을 WebGL에서 추론해 보기 (Unity Inference Engine/Sentis 등, 도입 전 사용자 확인)

0단계가 끝나면 이 체크리스트를 1단계(수직 슬라이스) 목록으로 바꾼다. 단계 정의는 GDD 8장 참고.

## 환경

| 항목 | 값 |
|---|---|
| Unity 에디터 | **6000.3.29f1** (Unity 6.3 LTS). 노트북과 데스크탑 모두 이 버전 고정, 업그레이드는 사용자 확인 후 |
| 렌더 파이프라인 | URP 2D (`Assets/Settings/Renderer2D.asset`) |
| 입력 | Input System (`Assets/Settings/InputSystem_Actions.inputactions`). 구 `Input.GetKey` 사용 금지 |
| 빌드 타깃 | WebGL (주), Windows IL2CPP (로컬 테스트용) |
| 버전 관리 | GitHub `jangjangtae/FiveNights` + Git LFS, 브랜치 `main` |
| 개발 기기 | 노트북 + 집 데스크탑 (GitHub로 동기화) |

- `com.unity.pipeline` 패키지는 Unity CLI가 에디터를 제어하는 데 쓰므로 **지우지 말 것**. 게임 코드에서는 참조하지 않는다.
- 새 패키지 추가나 프로젝트 설정의 큰 변경은 사용자에게 먼저 물어본다.

## 폴더 구조

```
Assets/
  _Project/                 # 직접 만든 에셋은 전부 이 아래
    Scripts/
      Core/                 # 게임 흐름, 밤 진행, 목숨, 점수, 경계도, 세이브
      Player/               # 이동, 웅크리기, 발소리, 도구 사용
      Guards/               # 경비 상태 머신, 순찰, 이동(경로 탐색)
      Perception/           # 시야, 청각, 의심 게이지 (감지 결과만 만든다)
      Memory/               # 침입 히트맵, 발각 위치 기록
      Strategy/             # 밤 사이 대응 결정 (순찰 추가, 카메라, 통로 잠금 등)
      UI/                   # HUD, 경비 보고서, 메뉴
      Utils/
    Scenes/
    Prefabs/
    Art/                    # Sprites, Tilesets, Materials, Lights
    Audio/
    ScriptableObjects/      # 경비 설정, 도구, 밤별 설정, 밸런스 수치
  Settings/                 # URP, Input 등 템플릿 설정 (위치 유지)
docs/                       # 기획서, 설계 메모
```

어셈블리 정의(asmdef)는 필요해질 때 사용자와 상의해서 추가한다.

## C# 코딩 규칙

- 네임스페이스: `FiveNights.<영역>` (예: `FiveNights.Guards`, `FiveNights.Perception`)
- **MonoBehaviour 하나당 파일 하나, 파일 이름 = 클래스 이름** (어기면 빌드한 플레이어가 시작할 때 크래시날 수 있음)
- 이름: 타입·메서드·프로퍼티 `PascalCase`, private 필드 `_camelCase`, 인스펙터 노출은 `[SerializeField] private`
- **데이터는 ScriptableObject로**: 속도, 시야각, 거리, 의심 증가율, 소리 반경, 경계도 수치 등을 코드에 하드코딩하지 않는다.
- **감지 → 판단 → 행동 분리** (나중에 RL 정책으로 교체할 수 있게):
  - `Perception`은 관측(본 것, 들은 것, 의심 수치)만 만든다.
  - 경비의 판단은 인터페이스(예: `IGuardBrain`) 뒤에 둔다. 1차 구현은 유한 상태 기계, 나중에 ML-Agents 정책으로 교체한다.
  - 행동(이동, 회전, 손전등)은 판단 결과를 실행만 한다. 판단 코드가 Transform이나 Rigidbody를 직접 만지지 않는다.
  - 같은 관측과 행동 형식을 규칙 경비와 RL 경비가 함께 쓴다.
- **그래픽과 로직 분리**: 게임 상태와 규칙은 순수 C# 클래스나 로직 컴포넌트에 두고, 스프라이트·라이트·이펙트·UI는 그 상태를 읽어서 그리기만 한다.
- 이동과 물리는 `Rigidbody2D` / `Collider2D`, 처리는 `FixedUpdate`에서.
- `Update`에서 `Find*`나 `GetComponent`를 반복 호출하지 말고 캐싱한다.
- 시야, 소리 반경, 경로, 상태 같은 AI 디버그 정보는 `OnDrawGizmos`와 디버그 표시로 항상 볼 수 있게 한다.
- 게임 로직(감지 판정, 히트맵, 전략 점수)은 EditMode 테스트를 작성할 수 있는 형태로 짠다.

### WebGL 제약

- `System.Threading`의 스레드와 로컬 파일 입출력(`System.IO`) 사용 금지. 세이브는 `PlayerPrefs`(JSON 직렬화)로 한다.
- 네트워크 호출은 `UnityWebRequest`만 쓴다.
- 텍스처 압축과 빌드 용량을 신경 쓴다. 빌드 결과물(`Build/`, `Builds/`)은 커밋하지 않는다.

## Claude 작업 방식 (Unity 에디터)

- Unity CLI: `%LOCALAPPDATA%\Microsoft\WindowsApps\unity.exe`
  - 에디터 명령마다 `--project-path "C:\Users\jang\Documents\FiveNights"`를 붙인다.
  - 작업 전에 `unity status`로 에디터가 `ready`인지 확인한다. 연결이 안 되면 사용자에게 에디터를 열어 달라고 한다.
- **씬과 프리팹은 에디터 명령으로 수정한다** (`unity command` 의 gameobject, component, scene, prefab, script eval 등). 에디터가 열려 있을 때 `.unity` / `.prefab` / `.asset` YAML을 직접 편집하지 않는다.
- 스크립트를 고친 뒤에는 컴파일 완료를 기다리고 콘솔을 확인한다 (`unity command console status`, `console get`). **에러 0개** 상태로 넘긴다.
- 기능을 만든 뒤에는 플레이 모드로 동작을 확인하고, 무엇을 어떻게 확인했는지 보고한다. 확인하지 못한 부분은 그렇다고 밝힌다.
- 사용자가 직접 플레이해 봐야 판단할 수 있는 것(손맛, 난이도, 재미)은 사용자에게 테스트를 부탁한다.
- 플레이 모드 자동 테스트 주의: 에디터 창에 포커스가 없으면 Input System이 키보드 입력을 막는다 (`editorInputBehaviorInPlayMode`). 자동 테스트는 `PlayerInputReader`를 끄고 값을 직접 넣어서 하고, 실제 키 입력은 사람이 확인한다. 이 설정을 바꾸려면 사용자에게 먼저 묻는다.
- CLI 호출 한 번에 1초 이상 걸려서 빠른 상황(추격 중 숨기 등)은 실시간으로 재현하기 어렵다. 판단 로직은 `FsmGuardBrain`에 가짜 관측을 넣는 시뮬레이션(script eval)으로 검증한다.
- 벽·엄폐물은 `Obstacle` 레이어(6번)에 둔다. 시야·청각·길찾기가 이 레이어만 장애물로 본다.
- 경비 루트 오브젝트는 회전시키지 않는다. 방향은 `GuardMover.Facing` 값으로만 관리하고 그래픽은 `GuardVisual`이 돌린다.
- 스크립트로 새 에셋을 만들고 같은 실행에서 씬에 연결하면 참조가 빠질 수 있다. 연결 후 씬 파일이나 인스펙터에서 참조가 들어갔는지 다시 확인한다.

## 역할별 서브 에이전트

`.claude/agents/`에 역할별 서브 에이전트가 있다. 노트북·데스크탑 어디서든 이 저장소에서 Claude Code를 열면 쓸 수 있다.

| 에이전트 | 역할 | 파일 수정 |
|---|---|---|
| `planner` | 기획. 아이디어를 작업 명세로 쪼개고 `docs/`를 고친다. 핵심 결정은 사용자에게 넘긴다 | `docs/`만 |
| `developer` | 구현. 스크립트·씬·프리팹, 컴파일 에러 0개까지 | 게임 코드·에셋 |
| `tester` | 완료 기준 확인. EditMode 테스트, 플레이 모드(CLI) 확인, 사람이 볼 항목 정리 | 테스트 코드만 |
| `reviewer` | 검토. CLAUDE.md 규칙·버그·WebGL 제약 | 없음 (읽기 전용) |

- `/feature <기능 설명>`: 기획 → 개발 → 테스트 → 검토 → 커밋·푸시를 한 번에 진행한다 (`.claude/commands/feature.md`).
- `/review [범위]`: 현재 변경을 검토만 한다.
- 직접 부를 때: "planner 에이전트로 ~ 기획해 줘"처럼 이름을 말한다.
- 서브 에이전트는 다른 서브 에이전트를 부를 수 없다. 순서 조율은 메인 세션이 한다.

## Git 규칙

- **작은 단위로 자주 커밋**한다. 메시지는 `<영역>: <무엇을 했는지>` (예: `Player: 웅크리기와 발소리 반경 추가`, `Perception: 시야 레이캐스트 가림 판정`).
- 커밋 전에 `git status`로 `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`가 들어가지 않았는지 확인한다.
- **기기를 바꾸기 전에 commit + push, 다른 기기에서 작업을 시작하기 전에 `git pull`.**
- 같은 씬을 두 기기에서 동시에 수정하지 않는다 (씬 YAML 충돌 방지).
- 이미지·오디오·폰트·3D 모델·psd·aseprite는 Git LFS로 관리한다 (`.gitattributes`). 새 바이너리 형식을 쓰게 되면 `.gitattributes`에 추가한다.
- 두 기기 모두 `git lfs install`이 되어 있어야 한다. clone 후 이미지가 텍스트 포인터로 보이면 `git lfs pull`.
