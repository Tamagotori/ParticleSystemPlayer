using UnityEngine;
using System.Collections.Generic;

namespace tamagotori.lib
{
    public partial class ParticleSystemPlayer
    {
        [System.Serializable]
        public class PhaseData
        {
            [Tooltip("フェーズ名")]
            public string phaseName;
            [Tooltip("パーティクル再生開始リスト")]
            public List<ParticleData> startParticleList = new List<ParticleData>();
            [Tooltip("パーティクル再生停止リスト")]
            public List<ParticleData> stopParticleList = new List<ParticleData>();
            [Tooltip("パーティクルクリアリスト")]
            public List<ParticleData> clearParticleList = new List<ParticleData>();
            [Tooltip("他のフェーズで扱っているStartパーティクルをクリア")]
            public bool clearOtherPhaseParticles;
        }

        [System.Serializable]
        public class JumpData
        {
            [Tooltip("フェーズ名")]
            public string phaseName;
            [Tooltip("ジャンプ先フェーズ名")]
            public string jumpPhaseName;
            [Tooltip("再生開始時間")]
            public float startTime;
        }

        [System.Serializable]
        public class ParticleData
        {
            [Tooltip("パーティクル")]
            public ParticleSystem particle;
            [Tooltip("実行ディレイタイム")]
            public float executeDelay;
        }

    }
}

