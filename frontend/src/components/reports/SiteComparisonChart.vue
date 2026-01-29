<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">Site Comparison</h3>
      <select v-model="metric" class="metric-select">
        <option value="agents">Total Agents</option>
        <option value="tiers">Tier Distribution</option>
        <option value="tenure">Avg Tenure</option>
      </select>
    </div>
    
    <div class="chart-container">
      <Bar :data="chartData" :options="chartOptions" />
    </div>

    <div class="site-cards">
      <div 
        v-for="(site, index) in sites" 
        :key="site.siteId"
        class="site-card"
        :style="{ borderColor: siteColors[index] }"
      >
        <h4 class="site-name">{{ site.siteName }}</h4>
        <div class="site-stats">
          <div class="site-stat">
            <span class="value">{{ site.totalAgents }}</span>
            <span class="label">Agents</span>
          </div>
          <div class="site-stat">
            <span class="value">{{ site.teamCount }}</span>
            <span class="label">Teams</span>
          </div>
          <div class="site-stat">
            <span class="value">{{ formatTenure(site.averageTenure) }}</span>
            <span class="label">Avg Tenure</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { Bar } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import type { SiteComparison } from '@/types/reports';
import { CHART_COLORS } from '@/types/reports';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const props = defineProps<{
  sites: SiteComparison[];
}>();

const metric = ref<'agents' | 'tiers' | 'tenure'>('agents');
const siteColors = CHART_COLORS.sites;

const chartData = computed(() => {
  const labels = props.sites.map(s => s.siteName);
  
  if (metric.value === 'agents') {
    return {
      labels,
      datasets: [
        {
          label: 'Active',
          data: props.sites.map(s => s.activeAgents),
          backgroundColor: CHART_COLORS.success,
          borderRadius: 6
        },
        {
          label: 'Training',
          data: props.sites.map(s => s.trainingAgents),
          backgroundColor: CHART_COLORS.warning,
          borderRadius: 6
        }
      ]
    };
  } else if (metric.value === 'tiers') {
    return {
      labels,
      datasets: [
        {
          label: 'Tier 1',
          data: props.sites.map(s => s.tier1Count),
          backgroundColor: CHART_COLORS.tier1,
          borderRadius: 6
        },
        {
          label: 'Tier 2',
          data: props.sites.map(s => s.tier2Count),
          backgroundColor: CHART_COLORS.tier2,
          borderRadius: 6
        },
        {
          label: 'Tier 3',
          data: props.sites.map(s => s.tier3Count),
          backgroundColor: CHART_COLORS.tier3,
          borderRadius: 6
        }
      ]
    };
  } else {
    return {
      labels,
      datasets: [{
        label: 'Avg Tenure (years)',
        data: props.sites.map(s => s.averageTenure),
        backgroundColor: props.sites.map((_, i) => siteColors[i % siteColors.length]),
        borderRadius: 6
      }]
    };
  }
});

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: metric.value !== 'tenure',
      position: 'top' as const,
      labels: {
        usePointStyle: true,
        padding: 16,
        font: { size: 12 }
      }
    },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      padding: 12,
      cornerRadius: 8
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { font: { size: 12 } }
    },
    y: {
      beginAtZero: true,
      grid: { color: '#f3f4f6' },
      ticks: { font: { size: 11 } }
    }
  }
}));

function formatTenure(years: number): string {
  if (years < 1) return `${Math.round(years * 12)}mo`;
  return `${years.toFixed(1)}yr`;
}
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

.metric-select {
  padding: 8px 12px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.85rem;
  color: #4b5563;
  background: white;
  cursor: pointer;
}

.metric-select:focus {
  outline: none;
  border-color: #3b82f6;
}

.chart-container {
  height: 280px;
  margin-bottom: 24px;
}

.site-cards {
  display: flex;
  gap: 16px;
}

.site-card {
  flex: 1;
  padding: 16px;
  border-radius: 12px;
  background: #f9fafb;
  border-left: 4px solid;
}

.site-name {
  font-size: 0.95rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 12px 0;
}

.site-stats {
  display: flex;
  gap: 16px;
}

.site-stat {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.site-stat .value {
  font-size: 1.1rem;
  font-weight: 700;
  color: #1f2937;
}

.site-stat .label {
  font-size: 0.7rem;
  color: #9ca3af;
  text-transform: uppercase;
}
</style>
