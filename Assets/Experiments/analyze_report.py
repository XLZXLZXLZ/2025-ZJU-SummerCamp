
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.font_manager as fm
import seaborn as sns
import numpy as np

# --- 字体设置 ---
def get_chinese_font():
    # 检查系统中是否存在 'SimHei' 字体
    font_list = fm.findSystemFonts(fontpaths=None, fontext='ttf')
    if any('SimHei' in font for font in font_list) or any('simhei' in font for font in font_list):
        return 'SimHei'
    print("警告：系统中未找到'SimHei'字体，将使用默认字体，中文可能无法正常显示。")
    return None

chinese_font = get_chinese_font()
if chinese_font:
    plt.rcParams['font.sans-serif'] = [chinese_font]
plt.rcParams['axes.unicode_minus'] = False
sns.set_theme(style="whitegrid", font=chinese_font)

# --- 数据加载和预处理 ---
try:
    df = pd.read_csv('Experiments/ExperimentReport_2025-07-09_17-50-02.csv')
except FileNotFoundError:
    print("错误：CSV文件未在 'Experiments/' 目录下找到。请确保文件路径正确。")
    exit()

df.dropna(how='all', inplace=True)
run_cols = [f'Run_{i}_Actual' for i in range(1, 6)]
expected_col = 'Expected_Result'
result_categories = ['CompletelyCorrect', 'PartiallyCorrect', 'Incorrect', 'Irrelevant']
df = df[df[expected_col].isin(result_categories)] # 过滤掉无效的预期结果行

# --- 1. 严格正确率统计 ---
strict_accuracy = {}
for category in result_categories:
    category_df = df[df[expected_col] == category]
    if not category_df.empty:
        total_runs = category_df.shape[0] * len(run_cols)
        correct_runs = (category_df[run_cols] == category).sum().sum()
        strict_accuracy[category] = correct_runs / total_runs if total_runs > 0 else 0

strict_df = pd.DataFrame(list(strict_accuracy.items()), columns=['标准答案', '正确率']).sort_values('标准答案')
strict_df['正确率'] = strict_df['正确率'] * 100

plt.figure(figsize=(10, 6))
barplot_strict = sns.barplot(x='正确率', y='标准答案', data=strict_df, palette='viridis', orient='h')
plt.title('1. 严格正确率统计', fontsize=16)
plt.xlabel('正确率 (%)', fontsize=12)
plt.ylabel('标准答案类型', fontsize=12)
plt.xlim(0, 105)
for index, row in strict_df.iterrows():
    barplot_strict.text(row['正确率'] + 1, index, f"{row['正确率']:.1f}%", color='black', ha="left", va='center')
plt.tight_layout()
plt.savefig('Experiments/1_strict_accuracy.png')
print("图表 '1_strict_accuracy.png' 已保存。")
plt.close()

# --- 2. 宽松正确率统计 ---
loose_accuracy = {}
for category in result_categories:
    category_df = df[df[expected_col] == category]
    if not category_df.empty:
        total_runs = category_df.shape[0] * len(run_cols)
        correct_runs = 0
        if category in ['CompletelyCorrect', 'PartiallyCorrect']:
            correct_runs = category_df[run_cols].isin(['CompletelyCorrect', 'PartiallyCorrect']).sum().sum()
        else: # Incorrect and Irrelevant
            correct_runs = (category_df[run_cols] == category).sum().sum()
        
        loose_accuracy[category] = correct_runs / total_runs if total_runs > 0 else 0

loose_df = pd.DataFrame(list(loose_accuracy.items()), columns=['标准答案', '正确率']).sort_values('标准答案')
loose_df['正确率'] = loose_df['正确率'] * 100

plt.figure(figsize=(10, 6))
barplot_loose = sns.barplot(x='正确率', y='标准答案', data=loose_df, palette='plasma', orient='h')
plt.title('2. 宽松正确率统计', fontsize=16)
plt.xlabel('正确率 (%)', fontsize=12)
plt.ylabel('标准答案类型', fontsize=12)
plt.xlim(0, 105)
for index, row in loose_df.iterrows():
    barplot_loose.text(row['正确率'] + 1, index, f"{row['正确率']:.1f}%", color='black', ha="left", va='center')
plt.tight_layout()
plt.savefig('Experiments/2_loose_accuracy.png')
print("图表 '2_loose_accuracy.png' 已保存。")
plt.close()

# --- 3. 回复分布统计 ---
melted_df = df.melt(id_vars=[expected_col], value_vars=run_cols, var_name='Run', value_name='Actual_Result')
crosstab = pd.crosstab(melted_df[expected_col], melted_df['Actual_Result'].fillna('Unknown'))
# 确保所有标准答案和实际结果的类别都作为索引/列存在
all_actual_cats = pd.unique(melted_df['Actual_Result'].dropna()).tolist()
all_cats = sorted(list(set(result_categories + all_actual_cats)))

crosstab = crosstab.reindex(index=result_categories, columns=all_cats, fill_value=0)

plt.figure(figsize=(12, 8))
sns.heatmap(crosstab, annot=True, fmt='d', cmap='YlGnBu', linewidths=.5)
plt.title('3. 回复分布统计 (标准答案 -> 实际回复)', fontsize=16)
plt.xlabel('实际生成的回复', fontsize=12)
plt.ylabel('预期的标准答案', fontsize=12)
plt.xticks(rotation=45, ha='right')
plt.yticks(rotation=0)
plt.tight_layout()
plt.savefig('Experiments/3_response_distribution.png')
print("图表 '3_response_distribution.png' 已保存。")
plt.close()

print("\n所有分析已完成，图表已保存到 'Experiments' 目录。") 