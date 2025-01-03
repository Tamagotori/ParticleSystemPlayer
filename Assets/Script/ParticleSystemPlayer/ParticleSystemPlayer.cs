using System.Collections.Generic;
using UnityEngine;

namespace tamagotori.lib
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public partial class ParticleSystemPlayer : MonoBehaviour
    {
        [Header("再生指定データ")]
        [Tooltip("再生開始フェーズ名\n空の場合は何もしない")]
        public string startPlayPhaseName;
        [Header("制御データ")]
        public List<PhaseData> phaseDataList = new List<PhaseData>();
        public List<JumpData> jumpDataList = new List<JumpData>();

        double m_currentTime;
        bool m_isPlaying = false;
        bool m_isPlayCurrentFrame = false;
        PhaseData m_currentPhase;
        List<ParticleSystem> m_playingParticleList = new List<ParticleSystem>();

        void OnEnable()
        {
            if (!string.IsNullOrEmpty(startPlayPhaseName) && !m_isPlayCurrentFrame)
            {
                Play(startPlayPhaseName);
            }
            m_isPlayCurrentFrame = false;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTime();
            UpdateParticleStatus();
        }

        public void Play(string phaseName)
        {
            if (string.IsNullOrEmpty(phaseName))
            {
                Debug.LogError("再生するフェーズ名が指定されていません");
                return;
            }

            var startTime = 0f;
            PhaseData phaseData = null;
            //jumpData
            foreach (var jumpData in jumpDataList)
            {
                if (jumpData.phaseName == phaseName)
                {
                    startTime = jumpData.startTime;
                    phaseName = jumpData.jumpPhaseName;
                    break;
                }
            }
            //phaseData
            foreach (var data in phaseDataList)
            {
                if (data.phaseName == phaseName)
                {
                    phaseData = data;
                    break;
                }
            }
            if (phaseData == null)
            {
                Debug.LogError("指定されたフェーズが存在しません");
                return;
            }
            if (gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
            }

            m_currentPhase = phaseData;
            m_currentTime = startTime;
            m_isPlaying = true;
            m_isPlayCurrentFrame = true;
            InitStartParticles();
            ClearOtherPhaseParticles();
            UpdateParticleStatus();
            AddUpdateSceneViewEvent();
        }

        public void Stop()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
            m_isPlaying = false;
            RemoveUpdateSceneViewEvent();
        }

        void UpdateTime()
        {
            if (m_isPlaying == false) return;
            m_currentTime += Time.deltaTime;
        }

        void InitStartParticles()
        {
            foreach (var particleData in m_currentPhase.startParticleList)
            {
                ClearParticle(particleData);
            }
        }

        void ClearOtherPhaseParticles()
        {
            if (!m_currentPhase.clearOtherPhaseParticles) return;
            foreach (var data in phaseDataList)
            {
                if (data == m_currentPhase) continue;
                foreach (var particleData in data.startParticleList)
                {
                    if (particleData.particle == null) continue;
                    ClearParticle(particleData);
                }
            }
        }

        void UpdateParticleStatus()
        {
            if (m_currentPhase == null) return;
            foreach (var data in m_currentPhase.startParticleList)
            {
                if (data.particle == null) continue;
                if (ExistPlayingParticle(data.particle)) continue;
                if (data.executeDelay >= m_currentTime) continue;
                PlayParticle(data);
            }
            foreach (var data in m_currentPhase.stopParticleList)
            {
                if (data.particle == null) continue;
                if (!ExistPlayingParticle(data.particle)) continue;
                if (data.executeDelay >= m_currentTime) continue;
                StopParticle(data);
            }
            foreach (var data in m_currentPhase.clearParticleList)
            {
                if (data.particle == null) continue;
                if (!ExistPlayingParticle(data.particle)) continue;
                if (data.executeDelay >= m_currentTime) continue;
                ClearParticle(data);
            }
        }

        void PlayParticle(ParticleData data)
        {
            data.particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            //最初は0fにしておかないとサブエミッター系が正常に動かない
            data.particle.Simulate(0f, true, true);
            data.particle.Simulate((float)m_currentTime - data.executeDelay, true, false);
            data.particle.Play(true);
            AddPlayingParticle(data.particle);
        }

        void StopParticle(ParticleData data)
        {
            data.particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            RemovePlayingParticle(data.particle);
        }

        void ClearParticle(ParticleData data)
        {
            data.particle.Clear(true);
            data.particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            RemovePlayingParticle(data.particle);
        }

        void AddPlayingParticle(ParticleSystem particle)
        {
            if (ExistPlayingParticle(particle)) return;
            m_playingParticleList.Add(particle);
        }

        void RemovePlayingParticle(ParticleSystem particle)
        {
            if (!ExistPlayingParticle(particle)) return;
            m_playingParticleList.Remove(particle);
        }

        bool ExistPlayingParticle(ParticleSystem particle)
        {
            if (m_playingParticleList == null) m_playingParticleList = new List<ParticleSystem>();
            return m_playingParticleList.Contains(particle);
        }

        void OnDestroy()
        {
            RemoveUpdateSceneViewEvent();
        }

        //EditorMode
        double m_prevDebugTime;

        void AddUpdateSceneViewEvent()
        {
#if UNITY_EDITOR
            m_prevDebugTime = UnityEditor.EditorApplication.timeSinceStartup;
            UnityEditor.EditorApplication.update -= UpdateSceneView;
            UnityEditor.EditorApplication.update += UpdateSceneView;
#endif
        }

        void RemoveUpdateSceneViewEvent()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= UpdateSceneView;
#endif
        }

        void UpdateSceneView()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdatePlayingParticleList();
                UnityEditor.SceneView.RepaintAll();
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }

        void UpdatePlayingParticleList()
        {
            var deltaTime = UnityEditor.EditorApplication.timeSinceStartup - m_prevDebugTime;
            m_prevDebugTime = UnityEditor.EditorApplication.timeSinceStartup;
            foreach (var phaseData in phaseDataList)
            {
                foreach (var particleData in phaseData.startParticleList)
                {
                    particleData.particle.Simulate((float)deltaTime, true, false);
                }
            }
        }

    }
}
