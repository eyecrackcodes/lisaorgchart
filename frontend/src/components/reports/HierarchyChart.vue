<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">Organization Hierarchy</h3>
    </div>
    
    <!-- Hierarchy Funnel -->
    <div class="hierarchy-funnel">
      <div 
        v-for="level in data.distribution" 
        :key="level.level"
        class="funnel-level"
        :style="{ width: `${getLevelWidth(level.level)}%` }"
      >
        <div class="level-bar" :class="`level-${level.level}`">
          <span class="level-label">{{ level.label }}</span>
          <span class="level-count">{{ level.count }}</span>
        </div>
      </div>
    </div>

    <!-- Span of Control -->
    <div class="span-section">
      <h4 class="section-title">Span of Control (Top Managers)</h4>
      <div class="span-chart">
        <div 
          v-for="manager in data.spanOfControl.slice(0, 8)" 
          :key="manager.managerId"
          class="span-item"
        >
          <div class="manager-info">
            <span class="manager-name">{{ truncateName(manager.managerName) }}</span>
            <span class="report-count">{{ manager.directReports }} reports</span>
          </div>
          <div class="span-bar">
            <div 
              class="span-fill"
              :style="{ 
                width: `${getSpanPercent(manager.directReports)}%`,
                backgroundColor: getSpanColor(manager.directReports)
              }"
            ></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Metrics -->
    <div class="hierarchy-metrics">
      <div class="metric">
        <span class="metric-value">{{ data.totalLevels }}</span>
        <span class="metric-label">Hierarchy Levels</span>
      </div>
      <div class="metric">
        <span class="metric-value">{{ averageSpan }}</span>
        <span class="metric-label">Avg Span of Control</span>
      </div>
      <div class="metric">
        <span class="metric-value">{{ maxSpan }}</span>
        <span class="metric-label">Max Direct Reports</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { HierarchyDepthData } from '@/types/reports';

const props = defineProps<{
  data: HierarchyDepthData;
}>();

function getLevelWidth(level: number): number {
  // Funnel shape: wider at top, narrower at bottom
  const widths = [100, 85, 70, 55, 40];
  return widths[level - 1] ?? 40;
}

const maxReports = computed(() => 
  Math.max(...props.data.spanOfControl.map(s => s.directReports), 1)
);

function getSpanPercent(reports: number): number {
  return (reports / maxReports.value) * 100;
}

function getSpanColor(reports: number): string {
  const percent = reports / maxReports.value;
  if (percent > 0.8) return '#ef4444';
  if (percent > 0.5) return '#f59e0b';
  return '#22c55e';
}

function truncateName(name: string): string {
  if (name.length > 20) {
    return name.substring(0, 18) + '...';
  }
  return name;
}

const averageSpan = computed(() => {
  const spans = props.data.spanOfControl.map(s => s.directReports);
  if (spans.length === 0) return 0;
  return Math.round(spans.reduce((a, b) => a + b, 0) / spans.length);
});

const maxSpan = computed(() => 
  Math.max(...props.data.spanOfControl.map(s => s.directReports), 0)
);
</script>

<style scoped>
.chart-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
}

.chart-header {
  margin-bottom: 24px;
}

.chart-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

/* Hierarchy Funnel */
.hierarchy-funnel {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  margin-bottom: 32px;
}

.funnel-level {
  transition: width 0.3s ease;
}

.level-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 20px;
  border-radius: 10px;
  color: white;
  font-weight: 600;
}

.level-bar.level-1 { background: linear-gradient(90deg, #0ea5e9, #38bdf8); }
.level-bar.level-2 { background: linear-gradient(90deg, #22c55e, #4ade80); }
.level-bar.level-3 { background: linear-gradient(90deg, #8b5cf6, #a78bfa); }
.level-bar.level-4 { background: linear-gradient(90deg, #6b7280, #9ca3af); }

.level-label {
  font-size: 0.9rem;
}

.level-count {
  font-size: 1.25rem;
  font-weight: 800;
}

/* Span of Control */
.span-section {
  padding: 24px 0;
  border-top: 1px solid #e5e7eb;
  border-bottom: 1px solid #e5e7eb;
  margin-bottom: 24px;
}

.section-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #6b7280;
  margin: 0 0 16px 0;
}

.span-chart {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.span-item {
  display: flex;
  align-items: center;
  gap: 12px;
}

.manager-info {
  width: 160px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.manager-name {
  font-size: 0.85rem;
  color: #1f2937;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.report-count {
  font-size: 0.7rem;
  color: #9ca3af;
}

.span-bar {
  flex: 1;
  height: 10px;
  background: #e5e7eb;
  border-radius: 5px;
  overflow: hidden;
}

.span-fill {
  height: 100%;
  border-radius: 5px;
  transition: width 0.5s ease;
}

/* Metrics */
.hierarchy-metrics {
  display: flex;
  justify-content: space-around;
}

.metric {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.metric-value {
  font-size: 1.75rem;
  font-weight: 800;
  color: #1f2937;
}

.metric-label {
  font-size: 0.75rem;
  color: #6b7280;
  text-align: center;
}
</style>
