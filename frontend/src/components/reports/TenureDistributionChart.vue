<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">Tenure Distribution</h3>
    </div>
    
    <div class="chart-container">
      <Bar :data="chartData" :options="chartOptions" />
    </div>

    <div class="tenure-breakdown">
      <div 
        v-for="range in data.ranges" 
        :key="range.label"
        class="tenure-bar"
      >
        <div class="bar-label">
          <span class="range">{{ range.label }}</span>
          <span class="count">{{ range.count }}</span>
        </div>
        <div class="bar-track">
          <div 
            class="bar-fill" 
            :style="{ width: `${range.percentage}%` }"
          ></div>
        </div>
        <span class="bar-percent">{{ Math.round(range.percentage) }}%</span>
      </div>
    </div>

    <div class="role-comparison">
      <h4 class="section-title">Average by Role</h4>
      <div class="role-bars">
        <div 
          v-for="role in data.averageByRole" 
          :key="role.role"
          class="role-item"
        >
          <span class="role-name">{{ role.role }}</span>
          <div class="role-bar">
            <div 
              class="role-fill"
              :style="{ width: `${getRolePercent(role.averageTenure)}%` }"
            ></div>
          </div>
          <span class="role-value">{{ formatTenure(role.averageTenure) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Bar } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Tooltip
} from 'chart.js';
import type { TenureDistributionData } from '@/types/reports';

ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip);

const props = defineProps<{
  data: TenureDistributionData;
}>();

const maxTenure = computed(() => 
  Math.max(...props.data.averageByRole.map(r => r.averageTenure), 3)
);

function getRolePercent(tenure: number): number {
  return (tenure / maxTenure.value) * 100;
}

function formatTenure(years: number): string {
  if (years < 1) return `${Math.round(years * 12)} months`;
  const y = Math.floor(years);
  const m = Math.round((years - y) * 12);
  if (m === 0) return `${y} year${y !== 1 ? 's' : ''}`;
  return `${y}y ${m}m`;
}

const chartData = computed(() => ({
  labels: props.data.ranges.map(r => r.label),
  datasets: [{
    label: 'Agents',
    data: props.data.ranges.map(r => r.count),
    backgroundColor: [
      'rgba(236, 72, 153, 0.8)',  // Pink - newest
      'rgba(249, 115, 22, 0.8)',  // Orange
      'rgba(245, 158, 11, 0.8)', // Amber
      'rgba(34, 197, 94, 0.8)',  // Green
      'rgba(59, 130, 246, 0.8)', // Blue
      'rgba(139, 92, 246, 0.8)'  // Purple - most tenured
    ],
    borderRadius: 8,
    borderSkipped: false
  }]
}));

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      padding: 12,
      cornerRadius: 8,
      callbacks: {
        label: (ctx: any) => ` ${ctx.raw} agents`
      }
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { 
        font: { size: 11 },
        maxRotation: 45,
        minRotation: 45
      }
    },
    y: {
      beginAtZero: true,
      grid: { color: '#f3f4f6' },
      ticks: { font: { size: 11 } }
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
  margin-bottom: 24px;
}

.chart-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.chart-container {
  height: 200px;
  margin-bottom: 32px;
}

.tenure-breakdown {
  display: none; /* Hidden since we're showing chart */
}

.role-comparison {
  padding-top: 24px;
  border-top: 1px solid #e5e7eb;
}

.section-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #6b7280;
  margin: 0 0 16px 0;
}

.role-bars {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.role-item {
  display: flex;
  align-items: center;
  gap: 16px;
}

.role-name {
  width: 80px;
  font-size: 0.85rem;
  color: #4b5563;
}

.role-bar {
  flex: 1;
  height: 12px;
  background: #e5e7eb;
  border-radius: 6px;
  overflow: hidden;
}

.role-fill {
  height: 100%;
  background: linear-gradient(90deg, #3b82f6, #8b5cf6);
  border-radius: 6px;
  transition: width 0.5s ease;
}

.role-value {
  width: 80px;
  text-align: right;
  font-size: 0.85rem;
  font-weight: 600;
  color: #1f2937;
}
</style>
