<template>
  <div class="org-chart-filters">
    <!-- Search -->
    <div class="filter-group search-group">
      <label class="filter-label">Search</label>
      <input
        type="text"
        class="filter-input"
        placeholder="Search by name..."
        v-model="searchTerm"
        @input="debouncedSearch"
      />
    </div>

    <!-- Site Filter -->
    <div class="filter-group">
      <label class="filter-label">Site</label>
      <select
        v-model="selectedSiteId"
        class="filter-select"
        @change="emitUpdate"
      >
        <option :value="undefined">All Sites</option>
        <option
          v-for="site in options.sites"
          :key="site.id"
          :value="Number(site.id)"
        >
          {{ site.name }}
        </option>
      </select>
    </div>

    <!-- Team Filter -->
    <div class="filter-group">
      <label class="filter-label">Team</label>
      <select
        v-model="selectedTeamId"
        class="filter-select"
        @change="emitUpdate"
      >
        <option :value="undefined">All Teams</option>
        <option
          v-for="team in options.teams"
          :key="team.id"
          :value="Number(team.id)"
        >
          {{ team.name }}
        </option>
      </select>
    </div>

    <!-- Manager Filter -->
    <div class="filter-group">
      <label class="filter-label">Manager</label>
      <select
        v-model="selectedManagerId"
        class="filter-select"
        @change="emitUpdate"
      >
        <option value="">All Managers</option>
        <option
          v-for="manager in options.managers"
          :key="manager.id"
          :value="manager.id"
        >
          {{ manager.name }}
        </option>
      </select>
    </div>

    <!-- Tag Filters -->
    <div class="filter-group tags-group">
      <label class="filter-label">Tags</label>
      <div class="tag-filters">
        <label
          v-for="tag in options.tags"
          :key="tag.id"
          class="tag-checkbox"
          :style="{
            '--tag-color': tag.hexColorCode
              ? `#${tag.hexColorCode}`
              : '#6b7280',
          }"
        >
          <input
            type="checkbox"
            :value="tag.id"
            v-model="selectedTagIds"
            @change="emitUpdate"
          />
          <span class="tag-label">{{ tag.name }}</span>
        </label>
      </div>
    </div>

    <!-- Include Inactive Toggle -->
    <div class="filter-group toggle-group">
      <label class="toggle-label">
        <input type="checkbox" v-model="includeInactive" @change="emitUpdate" />
        <span>Include Inactive</span>
      </label>
    </div>

    <!-- Reset Button -->
    <button class="reset-btn" @click="resetFilters">Reset Filters</button>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import type { OrgChartFilterOptions } from "@/types/orgChart";

const props = defineProps<{
  options: OrgChartFilterOptions;
}>();

const selectedSiteId = defineModel<number | undefined>("siteId");
const selectedTeamId = defineModel<number | undefined>("teamId");
const selectedManagerId = defineModel<string>("managerId", { default: "" });
const selectedTagIds = defineModel<number[]>("tagIds", { default: () => [] });
const includeInactive = defineModel<boolean>("includeInactive", {
  default: false,
});
const searchTerm = defineModel<string>("search", { default: "" });

const emit = defineEmits<{
  "update:siteId": [number | undefined];
  "update:teamId": [number | undefined];
  "update:managerId": [string];
  "update:tagIds": [number[]];
  "update:includeInactive": [boolean];
  "update:search": [string];
  reset: [];
}>();

let searchTimeout: number | undefined;

function debouncedSearch() {
  clearTimeout(searchTimeout);
  searchTimeout = window.setTimeout(() => {
    emit("update:search", searchTerm.value);
  }, 300);
}

function emitUpdate() {
  emit("update:siteId", selectedSiteId.value);
  emit("update:teamId", selectedTeamId.value);
  emit("update:managerId", selectedManagerId.value);
  emit("update:tagIds", selectedTagIds.value);
  emit("update:includeInactive", includeInactive.value);
}

function resetFilters() {
  selectedSiteId.value = undefined;
  selectedTeamId.value = undefined;
  selectedManagerId.value = "";
  selectedTagIds.value = [];
  includeInactive.value = false;
  searchTerm.value = "";
  emit("reset");
}
</script>

<style scoped>
.org-chart-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  padding: 16px;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  margin-bottom: 24px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.filter-input,
.filter-select {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  color: #1f2937;
  background: white;
  min-width: 150px;
  transition: border-color 0.15s ease;
}

.filter-input:focus,
.filter-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.search-group .filter-input {
  min-width: 200px;
}

.tags-group {
  flex: 1;
  min-width: 200px;
}

.tag-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.tag-checkbox {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.tag-checkbox input {
  accent-color: var(--tag-color);
}

.tag-label {
  font-size: 0.8rem;
  color: #4b5563;
  padding: 4px 8px;
  border: 1px solid var(--tag-color);
  border-radius: 4px;
  transition: all 0.15s ease;
}

.tag-checkbox input:checked + .tag-label {
  background: var(--tag-color);
  color: white;
}

.toggle-group {
  display: flex;
  align-items: flex-end;
}

.toggle-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 0.875rem;
  color: #4b5563;
}

.toggle-label input {
  width: 18px;
  height: 18px;
  accent-color: #3b82f6;
}

.reset-btn {
  align-self: flex-end;
  padding: 8px 16px;
  background: #f3f4f6;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.15s ease;
}

.reset-btn:hover {
  background: #e5e7eb;
  border-color: #9ca3af;
}

@media (max-width: 768px) {
  .org-chart-filters {
    flex-direction: column;
  }

  .filter-input,
  .filter-select {
    width: 100%;
  }
}
</style>
