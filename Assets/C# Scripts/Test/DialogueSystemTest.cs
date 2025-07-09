using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System;
using System.Linq;

namespace Game.Test
{
    // 用于存储单次测试结果的结构体
    public struct ExperimentResult
    {
        public int TestCaseIndex;
        public TestCase TestCase;
        public InferenceResult ActualResult;
    }

    /// <summary>
    /// 自动化对话系统测试脚本。
    /// </summary>
    public class DialogueSystemTest : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("要执行的实验预设。")]
        [SerializeField] private ExperimentData experimentPreset;
        [Tooltip("实验要重复执行的次数。")]
        [SerializeField] private int numberOfRuns = 5;

        private InferenceService _inferenceService;
        private StoryBlueprintSO _storyBlueprint;
        private List<ClueSO> _allClues;

        [ContextMenu("Run Dialogue System Experiment")]
        public void StartExperiment()
        {
            if (experimentPreset == null)
            {
                Debug.LogError("请先在Inspector中指定一个Experiment Preset！");
                return;
            }
            StartCoroutine(RunExperimentRoutine());
        }

        private IEnumerator RunExperimentRoutine()
        {
            if (!TryInitialize()) yield break;
            
            Debug.Log("实验开始...");
            var allResults = new List<ExperimentResult>();

            for (int run = 1; run <= numberOfRuns; run++)
            {
                Debug.Log($"第 {run}/{numberOfRuns} 轮测试开始...");
                for(int i = 0; i < experimentPreset.testCases.Count; i++)
                {
                    var testCase = experimentPreset.testCases[i];
                    bool isDone = false;
                    InferenceResult actualResult = new InferenceResult();

                    ClueSO clue = _allClues.Find(c => c.clueID == testCase.clueID);
                    if (clue == null)
                    {
                        Debug.LogError($"错误: 找不到ID为'{testCase.clueID}'的线索。");
                        continue;
                    }

                    _inferenceService.AskQuestionAboutClue(_storyBlueprint, clue, testCase.playerQuestion, (result) =>
                    {
                        actualResult = result;
                        isDone = true;
                    });
                    
                    yield return new WaitUntil(() => isDone);
                    
                    allResults.Add(new ExperimentResult
                    {
                        TestCaseIndex = i,
                        TestCase = testCase,
                        ActualResult = actualResult
                    });
                }
                Debug.Log($"第 {run}/{numberOfRuns} 轮测试结束。");
            }

            Debug.Log("实验结束。正在生成报告...");
            GenerateCSVReport(allResults);
        }

        private bool TryInitialize()
        {
            _inferenceService = new InferenceService();
            _storyBlueprint = GameManager.Instance.currentStory;
            _allClues = FindObjectOfType<ClueUIPanel>()?.GetAllCluesForTest();
            if (_storyBlueprint == null || _allClues == null)
            {
                Debug.LogError("无法找到GameManager或ClueUIPanel，或者它们没有配置好故事/线索数据！");
                return false;
            }
            return true;
        }

        private void GenerateCSVReport(List<ExperimentResult> allResults)
        {
            var csvBuilder = new StringBuilder();
            
            // 构建CSV表头
            string header = "TestCase_ID,Question,Clue_ID,Expected_Result,";
            for(int i = 1; i <= numberOfRuns; i++)
            {
                header += $"Run_{i}_Actual,Run_{i}_Explanation,";
            }
            header += "Success_Rate,Most_Common_Result";
            csvBuilder.AppendLine(header);

            // 按每个测试用例分组
            var groupedResults = allResults.GroupBy(res => res.TestCaseIndex);

            foreach (var group in groupedResults)
            {
                var testCase = group.First().TestCase;
                var line = new List<string>
                {
                    group.Key.ToString(),
                    $"\"{testCase.playerQuestion.Replace("\"", "\"\"")}\"", // 处理引号
                    testCase.clueID,
                    testCase.expectedResult.ToString()
                };

                int successCount = 0;
                var actuals = new List<EvaluationResult>();

                foreach (var result in group)
                {
                    line.Add(result.ActualResult.evaluation.ToString());
                    line.Add($"\"{result.ActualResult.explanation?.Replace("\"", "\"\"")}\"");
                    actuals.Add(result.ActualResult.evaluation);
                    if (result.ActualResult.evaluation == testCase.expectedResult)
                    {
                        successCount++;
                    }
                }
                
                // 计算分析数据
                float successRate = (float)successCount / numberOfRuns;
                var mostCommonResult = actuals.GroupBy(r => r)
                                              .OrderByDescending(g => g.Count())
                                              .First().Key;

                line.Add(successRate.ToString("P0")); // 格式化为百分比
                line.Add(mostCommonResult.ToString());

                csvBuilder.AppendLine(string.Join(",", line));
            }
            
            // 写入文件
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string folderPath = Path.Combine(Application.dataPath, "../Logs");
                string filePath = Path.Combine(folderPath, $"ExperimentReport_{timestamp}.csv");

                new FileInfo(filePath).Directory?.Create();
                File.WriteAllText(filePath, csvBuilder.ToString(), Encoding.UTF8);
                Debug.Log($"CSV报告已保存到: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"无法写入CSV报告文件: {e.Message}");
            }
        }
    }
} 