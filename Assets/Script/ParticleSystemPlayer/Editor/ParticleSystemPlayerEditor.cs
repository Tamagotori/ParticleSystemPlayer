using UnityEngine;
using UnityEditor;

namespace tamagotori.lib
{
    [CustomEditor(typeof(ParticleSystemPlayer))]
    public class ParticleSystemPlayerEditor : Editor
    {
        string m_debugPhaseName;
        string m_text = "";
        public override void OnInspectorGUI()
        {
            DrawMessage();

            base.OnInspectorGUI();

            DrawHorizontalLine();

            GUILayout.Label("確認再生", EditorStyles.boldLabel);
            m_debugPhaseName = EditorGUILayout.TextField("デバッグ再生名", m_debugPhaseName);

            if (GUILayout.Button("デバッグ再生"))
            {
                DebugPlay();
            }
            if (GUILayout.Button("デバッグ停止"))
            {
                DebugStop();
            }

            GUILayout.Space(10);
            GUILayout.Label("編集機能", EditorStyles.boldLabel);
            if (GUILayout.Button("空Particle生成"))
            {
                CreateEmptyParticle();
            }
        }

        void DrawMessage()
        {
            m_text = "";
            var player = (ParticleSystemPlayer)target;

            CheckPhaseName(player);
            CheckSetParticle(player);
            CheckExistPhaseName(player, player.startPlayPhaseName, "再生開始フェーズ名");
            CheckExistPhaseName(player, m_debugPhaseName, "デバッグフェーズ名");

            //output
            if (string.IsNullOrEmpty(m_text))
            {
                EditorGUILayout.HelpBox($"入力データに問題なし", MessageType.None);
            }
            else
            {
                m_text = m_text.TrimEnd('\n');
                EditorGUILayout.HelpBox($"{m_text}", MessageType.Warning);
            }

        }

        void CheckPhaseName(ParticleSystemPlayer player)
        {
            //check phase
            var index = 0;
            foreach (var data in player.phaseDataList)
            {
                if (string.IsNullOrEmpty(data.phaseName))
                {
                    m_text += $"フェーズ名が未定義:{index}\n";
                }
                index++;
            }
            index = 0;
            foreach (var data in player.jumpDataList)
            {
                if (string.IsNullOrEmpty(data.phaseName))
                {
                    m_text += $"ジャンプ元フェーズ名が未定義:{index}\n";
                }
                if (string.IsNullOrEmpty(data.jumpPhaseName))
                {
                    m_text += $"ジャンプ先フェーズ名が未定義:{index}\n";
                }
                index++;
            }
        }

        void CheckSetParticle(ParticleSystemPlayer player)
        {

            //check set particle
            foreach (var data in player.phaseDataList)
            {
                foreach (var particleData in data.startParticleList)
                {
                    if (particleData.particle == null)
                    {
                        m_text += $"パーティクルが未設定:{data.phaseName}\n";
                    }
                }
                foreach (var particleData in data.stopParticleList)
                {
                    if (particleData.particle == null)
                    {
                        m_text += $"パーティクルが未設定:{data.phaseName}\n";
                    }
                }
                foreach (var particleData in data.clearParticleList)
                {
                    if (particleData.particle == null)
                    {
                        m_text += $"パーティクルが未設定:{data.phaseName}\n";
                    }
                }
            }
        }

        void CheckExistPhaseName(ParticleSystemPlayer player, string phaseName, string msgName)
        {
            if (string.IsNullOrEmpty(phaseName)) return;
            foreach (var data in player.phaseDataList)
            {
                if (data.phaseName == phaseName) return;
            }
            foreach (var data in player.jumpDataList)
            {
                if (data.phaseName == phaseName) return;
            }
            m_text += $"存在しない{msgName}:{phaseName}\n";
        }

        public void DebugPlay()
        {
            var player = (ParticleSystemPlayer)target;
            player.Play(m_debugPhaseName);
        }

        public void DebugStop()
        {
            var player = (ParticleSystemPlayer)target;
            player.Stop();
        }

        public void CreateEmptyParticle()
        {
            var obj = new GameObject("EmptyParticle");
            //transform
            obj.transform.parent = ((ParticleSystemPlayer)target).transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            //particle
            var particle = obj.AddComponent<ParticleSystem>();
            var main = particle.main;
            main.stopAction = ParticleSystemStopAction.Disable;
            var emission = particle.emission;
            emission.enabled = false;
            var shape = particle.shape;
            shape.enabled = false;
            var particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
            particleRenderer.enabled = false;
        }

        private void DrawHorizontalLine()
        {
            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);  // 指定した高さの領域を確保
            EditorGUI.DrawRect(rect, Color.gray);  // 指定した色で矩形を描画
            GUILayout.Space(10);
        }

    }

}
