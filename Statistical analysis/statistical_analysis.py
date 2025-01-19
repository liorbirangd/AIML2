import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from scipy.stats import ttest_ind
from scipy.stats import f_oneway

def load_and_prepare_data(file_path):
    data = pd.read_csv(file_path)
    data.columns = data.columns.str.strip()
    return data

def calculate_window_metrics(data, step_column="Step", window_size=500000):
    results_means = []
    results_slopes = []
    groups = [col for col in data.columns if col != step_column]
    max_step = data[step_column].max()

    for group in groups:
        group_data = data[[step_column, group]].dropna()
        group_means = []
        group_slopes = []

        for start in range(0, int(max_step), window_size):
            end = start + window_size
            window_data = group_data[(group_data[step_column] >= start) & (group_data[step_column] < end)]

            if len(window_data) > 1:
                mean_value = window_data[group].mean()
                slope = np.polyfit(window_data[step_column], window_data[group], 1)[0]
                group_means.append(mean_value)
                group_slopes.append(slope)

        results_means.append({"Group": group, "Window Metrics": group_means})
        results_slopes.append({"Group": group, "Window Metrics": group_slopes})

    return results_means, results_slopes

def create_window_table(window_metrics, metric_type, window_size):
    pd.options.display.max_columns = None
    table_data = {}

    for group_data in window_metrics:
        group = group_data["Group"]
        metrics = group_data["Window Metrics"]
        window_labels = [f"{i * window_size}-{(i + 1) * window_size}" for i in range(len(metrics))]
        table_data[group] = pd.Series(metrics, index=window_labels)

    table_df = pd.DataFrame(table_data)
    print(f"\nWindow {metric_type} Table:")
    print(table_df.to_string())  # Ensure full table is printed
    return table_df

def rank_groups_in_windows(window_metrics, minimize_metric):
    group_points = {group_data["Group"]: 0 for group_data in window_metrics}

    for window_idx in range(len(window_metrics[0]["Window Metrics"])):
        window_values = []

        for group_data in window_metrics:
            group = group_data["Group"]
            metrics = group_data["Window Metrics"]
            if window_idx < len(metrics):
                window_values.append((group, metrics[window_idx]))

        # Sort groups based on metric value
        sorted_values = sorted(window_values, key=lambda x: x[1], reverse=not minimize_metric)

        # Assign points based on ranking
        for rank, (group, _) in enumerate(sorted_values):
            group_points[group] += len(sorted_values) - rank

    return group_points

def identify_best_group(window_metrics, group_points, minimize_metric):
    # Determine the group with the highest points
    max_points = max(group_points.values())
    candidates = [group for group, points in group_points.items() if points == max_points]

    if len(candidates) == 1:
        best_group = candidates[0]
    else:
        # Break tie by calculating the overall mean for tied groups
        group_means = {}
        for group_data in window_metrics:
            group = group_data["Group"]
            if group in candidates:
                group_means[group] = np.nanmean(group_data["Window Metrics"])

        best_group = min(group_means, key=group_means.get) if minimize_metric else max(group_means, key=group_means.get)

    overall_mean = np.nanmean(next(g["Window Metrics"] for g in window_metrics if g["Group"] == best_group))
    print(f"\nBest Overall Group: {best_group} with Points = {group_points[best_group]} and Average Value = {overall_mean:.4f}")
    return best_group

def perform_anova_on_windows(window_table):
    # Transpose the table for ANOVA: columns are groups, rows are windows
    group_values = [window_table[col].dropna().values for col in window_table.columns]

    # Perform ANOVA
    if len(group_values) > 1:
        f_stat, p_value = f_oneway(*group_values)
        print(f"ANOVA Results: F-statistic = {f_stat:.4f}, P-value = {p_value:.4f}")

        if p_value < 0.05:
            print("There is a significant difference between groups.")
        else:
            print("No significant difference between groups.")
    else:
        print("ANOVA could not be performed due to insufficient data.")

def perform_pairwise_ttests(best_group, window_metrics, metric_type):
    best_group_data = next((g["Window Metrics"] for g in window_metrics if g["Group"] == best_group), [])
    results = []
    significant = True

    for group_data in window_metrics:
        group = group_data["Group"]
        if group != best_group:
            other_data = group_data["Window Metrics"]
            if len(best_group_data) < 2 or len(other_data) < 2:
                t_stat, p_val = np.nan, np.nan
                significant = False
            else:
                t_stat, p_val = ttest_ind(best_group_data, other_data, equal_var=False)
                if p_val >= 0.05:
                    significant = False

            results.append({
                "Comparison": f"{best_group} vs {group}",
                "T-statistic": t_stat,
                "P-value": p_val,
                "Significant": p_val < 0.05 if not np.isnan(p_val) else False
            })

    print(f"\nPairwise T-Tests for {metric_type}:")
    for result in results:
        print(result)

    if significant:
        print(f"\nThe group '{best_group}' is statistically significantly better than all other groups for {metric_type}.")
    else:
        print(f"\nThe group '{best_group}' is NOT statistically significantly better than all other groups for {metric_type}.")

    return results

def plot_summary_metrics(window_metrics, metric_column_label, window_size):
    plt.figure(figsize=(12, 6))

    for group_data in window_metrics:
        group = group_data["Group"]
        metrics = group_data["Window Metrics"]
        window_labels = [f"{i * window_size}-{(i + 1) * window_size}" for i in range(len(metrics))]
        plt.plot(window_labels, metrics, marker="o", label=f"{group}")

    plt.title(f"{metric_column_label} Metrics Across Windows")
    plt.xlabel("Window Steps")
    plt.ylabel(metric_column_label)
    plt.legend()
    plt.grid()
    plt.tight_layout()
    plt.show()

def main():
    file_path = "time_horizon_elo.csv"
    metric_column_label = "ELO"
    step_column = "Step"
    window_size = 500000
    minimize_metric = False

    data = load_and_prepare_data(file_path)

    # Calculate window metrics
    window_means, window_slopes = calculate_window_metrics(data, step_column, window_size)

    # Create and display window metrics tables
    print("\nCalculating Mean Metrics:")
    mean_table = create_window_table(window_means, "Mean", window_size)
    # Preform ANOVA test on means
    print("\nPerforming ANOVA test for Means:")
    perform_anova_on_windows(mean_table)
    # Rank groups and determine the best group for means
    print("\nRanking Groups for Means:")
    group_points_means = rank_groups_in_windows(window_means, minimize_metric)
    best_group_means = identify_best_group(window_means, group_points_means, minimize_metric)
    # Perform pairwise t-tests between the best group and others
    print("\nPerforming Pairwise T-Tests for Means:")
    perform_pairwise_ttests(best_group_means, window_means, "Means")
    # Plot summary metrics
    print("\nPlotting Mean Metrics:")
    plot_summary_metrics(window_means, f"{metric_column_label} Mean", window_size)

    print("--------------------------------------------------------------------------------------------------------------------------------------------------------------")
    # Create and display window metrics tables
    print("\nCalculating Slope Metrics:")
    slope_table = create_window_table(window_slopes, "Slope", window_size)
    # Preform ANOVA test on slopes
    print("\nPerforming ANOVA test for Slopes:")
    perform_anova_on_windows(slope_table)
    # Rank groups and determine the best group for slopes
    print("\nRanking Groups for Slopes:")
    group_points_slopes = rank_groups_in_windows(window_slopes, minimize_metric)
    best_group_slopes = identify_best_group(window_slopes, group_points_slopes, minimize_metric)
    # Perform pairwise t-tests between the best group and others
    print("\nPerforming Pairwise T-Tests for Slopes:")
    perform_pairwise_ttests(best_group_slopes, window_slopes, "Slopes")
    # Plot summary metrics
    print("\nPlotting Slope Metrics:")
    plot_summary_metrics(window_slopes, f"{metric_column_label} Slope", window_size)

if __name__ == "__main__":
    main()
