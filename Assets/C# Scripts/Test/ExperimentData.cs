using System.Collections.Generic;
using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 定义一次自动化测试中的单个测试用例。
    /// </summary>
    [System.Serializable]
    public struct TestCase
    {
        [Tooltip("要测试的线索的ID。")]
        public string clueID;
        [Tooltip("模拟玩家提出的问题。")]
        public string playerQuestion;
        [Tooltip("我们期望LLM返回的评估结果。")]
        public EvaluationResult expectedResult;
    }

    /// <summary>
    /// 一个包含了一整套测试用例的ScriptableObject资源。
    /// 允许你在Project窗口中创建和管理不同的测试集。
    /// </summary>
    [CreateAssetMenu(fileName = "NewExperimentPreset", menuName = "Game/Testing/Experiment Preset")]
    public class ExperimentData : ScriptableObject
    {
        [Tooltip("此测试预设中包含的所有测试用例。")]
        public List<TestCase> testCases;
    }
} 