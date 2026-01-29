<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">Commission Tier Distribution</h3>
      <div class="chart-legend">
        <span class="legend-item tier1"><span class="dot"></span> Tier 1</span>
        <span class="legend-item tier2"><span class="dot"></span> Tier 2</span>
        <span class="legend-item tier3"><span class="dot"></span> Tier 3</span>
      </div>
    </div>
    
    <div class="chart-container">
      <Doughnut :data="chartData" :options="chartOptions" />
    </div>

    <div class="chart-stats">
      <div class="stat-item">
        <span class="stat-value tier1">{{ data.tier1 }}</span>
        <span class="stat-label">Tier 1</span>
        <span class="stat-percent">{{ getPercent(data.tier1) }}%</span>
      </div>
      <div class="stat-item">
        <span class="stat-value tier2">{{ data.tier2 }}</span>
        <span class="stat-label">Tier 2</span>
        <span class="stat-percent">{{ getPercent(data.tier2) }}%</span>
      </div>
      <div class="stat-item">
        <span class="stat-value tier3">{{ data.tier3 }}</span>
        <span class="stat-label">Tier 3</span>
        <span class="stat-percent">{{ getPercent(data.tier3) }}%</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Doughnut } from 'vue-chartjs';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend
} from 'chart.js';
import type { TierDistributionData } from '@/types/reports';
import { CHART_COLORS } from '@/types/reports';

ChartJS.register(ArcElement, Tooltip, Legend);

const props = defineProps<{
  data: TierDistributionData;
}>();

const total = computed(() => 
  props.data.tier1 + props.data.tier2 + props.data.tier3 + props.data.none
);

function getPercent(value: number): number {
  return total.value > 0 ? Math.round((value / total.value) * 100) : 0;
}

const chartData = computed(() => ({
  labels: ['Tier 1', 'Tier 2', 'Tier 3', 'None'],
  datasets: [{
    data: [props.data.tier1, props.data.tier2, props.data.tier3, props.data.none],
    backgroundColor: [
      CHART_COLORS.tier1,
      CHART_COLORS.tier2,
      CHART_COLORS.tier3,
      CHART_COLORS.gray
    ],
    borderWidth: 0,
    hoverOffset: 8
  }]
}));

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '65%',
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleFont: { size: 14, weight: 'bold' as const },
      bodyFont: { size: 13 },
      padding: 12,
      cornerRadius: 8,
      callbacks: {
        label: (ctx: any) => {
          const value = ctx.raw;
          const percent = getPercent(value);
          return ` ${value} agents (${percent}%)`;
        }
      }
    }
  }
};
</script>

<style scoped>
.chart-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.chart-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.chart-legend {
  display: flex;
  gap: 16px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  color: #6b7280;
}

.legend-item .dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.legend-item.tier1 .dot { background: #22c55e; }
.legend-item.tier2 .dot { background: #3b82f6; }
.legend-item.tier3 .dot { background: #8b5cf6; }

.chart-container {
  height: 220px;
  margin-bottom: 24px;
}

.chart-stats {
  display: flex;
  justify-content: center;
  gap: 32px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 800;
}

.stat-value.tier1 { color: #22c55e; }
.stat-value.tier2 { color: #3b82f6; }
.stat-value.tier3 { color: #8b5cf6; }

.stat-label {
  font-size: 0.8rem;
  color: #6b7280;
}

.stat-percent {
  font-size: 0.75rem;
  color: #9ca3af;
  background: #f3f4f6;
  padding: 2px 8px;
  border-radius: 10px;
}
</style>
