# RapidFramework : DataStep

> **RapidFramework-DataStep**은 `RapidFramework-Core`에서 데이터 관련 핵심 스텝을 분리하여, 프로그래밍 아키텍처 및 설계에 대한 이해를 돕기 위해 공개된 **'레퍼런스 프로젝트'**입니다.

본 프로젝트는 기본적인 프레임워크 설계와 구조적인 데이터 핸들링(Data Handling)의 이해를 돕기 위해 제작되었습니다. 

프로그래밍 설계를 입문하거나 아키텍처를 공부하시는 분들에게 도움이 되기를 바랍니다. 

모두들 좋은 하루 되시고, 프로젝트에 도움이 되시길 바랍니다. 감사합니다!

* **Contact**: [유니티 허브 네이버 카페] - **오형남자**
* **E-Mail**: windyflows@naver.com / windyflows@gmail.com

---

## 목적 (Purpose)

* **생산성 최우선**: RapidFramework는 이름 그대로 '생산성(Productivity)'을 극대화하기 위한 프레임워크입니다. 기술적인 최적화 비율보다 개발 생산성을 높이는 데 초점을 맞추어 설계되었습니다.
 
* **검증된 프레임워크**: V1, V2 버전을 거쳐 실제 **'라이브 서비스 프로젝트'**를 성공적으로 릴리즈(Release)했던 노하우를 담았습니다. 이번 V3 버전 역시 라이브 프로젝트를 통해 안정성이 검증되었습니다.

---

## 요약 (Summary)

* **SMO 구조**: 중앙집중형 아키텍처인 **System, Manager, Object (SMO)** 구조를 채택하고 있습니다.
  
* **계층적 분리**: 하위 Manager들은 `Service`와 `Provider`라는 네이밍을 통해 책임과 개념을 명확히 분리합니다.
  
* **ParserManager**: `Newtonsoft.Json`을 사용하여 모든 데이터를 범용적인 JSON 포멧으로 파싱합니다.
  
* **DataManager**: Excel(`Xlsx`) 시트를 기반으로 Local Database를 구축하고 데이터를 효율적으로 관리합니다.
  
* **Observable**: 동적 데이터의 변경을 감시하고 이벤트를 발행(Notification)하는 클래스입니다.
  
* **Registrable**: 동적 객체의 상태 변경을 감시하고 관리하는 클래스입니다.

> ⚠️ **주의 사항**
> * 본 프로젝트는 `RapidFramework-Core`에서 **DataStep 부분만 분리**하여 작성되었기 때문에, 코어 레벨과 연결된 일부 구조가 누락되어 있을 수 있습니다. 복합적인 연계 구조는 본 레퍼런스에서 다루지 않습니다.
> * RapidFramework-V3는 **Unity 6** 환경 및 최신 **C# 코드 컨벤션/디자인 패턴**을 준수합니다.

---

## 🛠️ 코드 컨벤션 및 규칙 (Code Convention & Rules)

* **블랙박스화**: 기본적으로 보일러플레이트(Boilerplate) 코드를 최소화하고, 내부 로직을 캡슐화(Blackbox)하는 것을 지향합니다.
  
* **보호 구문 준수**: `Guard Clause` 및 `Early Return` 패턴을 엄격히 준수하며, 가독성을 위해 Early Return문 뒤에는 **개행(Line Break)**을 추가합니다. (`Guard.cs` 활용)
  
* **멤버 변수**: 클래스 내 멤버 변수는 접두사 `_`를 사용합니다. (예: `_currentHp`)
  
* **이벤트 네이밍**: 이벤트 성격의 변수나 함수는 접두사 `on`을 붙여 명시합니다. (예: `_onTargetChange`, 함수에서는 OnTargetChange 등)
  
* **파라미터 개행**: 함수의 파라미터가 3개 이상이거나 구조가 복잡할 경우, 개행을 통해 가독성을 높입니다.
  
* **생산성 도구 활용**: `Facade`, `Editor`, `Event`, `AnimationCurve(Easing)` 등의 기법을 적극 활용하여 개발 팀 내의 생산성을 끌어올립니다.
  
* **엔진 연계**: 코드 전용 추적 방식에만 의존하지 않고, **Unity Engine** 컴포넌트 구조와 유연하게 연계되도록 로직을 구성합니다.

---

## RapidFramework - Core 아키텍처

> 아래 목록은 `RapidFramework-DataStep` 레퍼런스에는 포함되어 있지 않지만, 상위 `RapidFramework-Core` 계층에서 지원하는 전체 모듈 라인업입니다.

### Managers
* `AddressableManager`
*  `AIManager`
* `CameraManager`
* `DataManager`
* `EventManager`
* `FirebaseManager`
* `HardwareManager`
* `InputManager`
* `LogManager`
* `NetworkManager`
* `ParserManager`
* `PoolManager`
* `QuestManager`
* `RandomManager`
* `SceneManager`
* `SecurityManager`
* `SequenceManager`
* `SoundManager`
* `SpawnManager`
* `TimeManager`
* `UIManager`

### Objects & UI
* **Objects**:
* `Draggable`
* `Observable`
* `Register`
* `VideoPlayer`
  
* **UI**:
* `DialogueUI`
* `ToastUI`

### Effects & Events
* **Effects**:
* `Easing`,
* `TextTyping`
* `HangulConverter`
* `TextEffects`
* `BasicEffects (Position, Typing, ScreenOutside... etc)`
  
* **Events**:
* `DelayEvent`
* `InputKeyEvent`

### Support & Extensions
* **Support**:
* `AdaptiveCanvasScaler`
* `AdaptiveCameraScaler`
