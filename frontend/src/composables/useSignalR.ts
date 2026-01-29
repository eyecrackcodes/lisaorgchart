/**
 * SignalR composable for real-time org chart updates
 */

import { ref, onMounted, onUnmounted, type Ref } from 'vue';
import * as signalR from '@microsoft/signalr';
import type { GraphNodeProjection, GraphNodeDetail, GraphQuery, GraphNodeStats } from '@/types/graph';

const hubUrl = import.meta.env.VITE_API_BASE_URL 
  ? `${import.meta.env.VITE_API_BASE_URL}/hubs/org-chart`
  : 'http://localhost:5000/hubs/org-chart';

// Singleton connection
let connection: signalR.HubConnection | null = null;
const connectionState = ref<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
const connectionError = ref<Error | null>(null);

export function useSignalR() {
  const isConnected = ref(false);
  
  // Event callbacks
  const onNodeUpdated = ref<((node: GraphNodeProjection) => void) | null>(null);
  const onNodeDeleted = ref<((nodeId: string) => void) | null>(null);
  const onNodeCreated = ref<((node: GraphNodeProjection) => void) | null>(null);
  const onStatsUpdated = ref<((data: { nodeId: string; stats: GraphNodeStats }) => void) | null>(null);
  const onFilterApplied = ref<((data: { query: GraphQuery; results: GraphNodeProjection[] }) => void) | null>(null);

  async function connect() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
      isConnected.value = true;
      return;
    }

    try {
      connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          withCredentials: true
        })
        .withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Register event handlers
      connection.on('NodeUpdated', (node: GraphNodeProjection) => {
        onNodeUpdated.value?.(node);
      });

      connection.on('NodeDeleted', (nodeId: string) => {
        onNodeDeleted.value?.(nodeId);
      });

      connection.on('NodeCreated', (node: GraphNodeProjection) => {
        onNodeCreated.value?.(node);
      });

      connection.on('ChildUpdated', (node: GraphNodeProjection) => {
        onNodeUpdated.value?.(node);
      });

      connection.on('StatsUpdated', (data: { nodeId: string; stats: GraphNodeStats }) => {
        onStatsUpdated.value?.(data);
      });

      connection.on('FilterApplied', (data: { query: GraphQuery; results: GraphNodeProjection[] }) => {
        onFilterApplied.value?.(data);
      });

      connection.onreconnecting(() => {
        connectionState.value = signalR.HubConnectionState.Reconnecting;
        isConnected.value = false;
      });

      connection.onreconnected(() => {
        connectionState.value = signalR.HubConnectionState.Connected;
        isConnected.value = true;
        connectionError.value = null;
      });

      connection.onclose(() => {
        connectionState.value = signalR.HubConnectionState.Disconnected;
        isConnected.value = false;
      });

      await connection.start();
      connectionState.value = signalR.HubConnectionState.Connected;
      isConnected.value = true;
      connectionError.value = null;
      console.log('SignalR connected');
    } catch (error) {
      connectionError.value = error as Error;
      console.error('SignalR connection failed:', error);
    }
  }

  async function disconnect() {
    if (connection) {
      await connection.stop();
      isConnected.value = false;
    }
  }

  // Hub methods
  async function subscribeToNode(nodeId: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('SubscribeToNode', nodeId);
    }
  }

  async function unsubscribeFromNode(nodeId: string) {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('UnsubscribeFromNode', nodeId);
    }
  }

  async function subscribeToAll() {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke('SubscribeToAll');
    }
  }

  async function requestChildren(nodeId: string, query: GraphQuery = {}): Promise<GraphNodeProjection[]> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      return await connection.invoke('RequestChildren', nodeId, query);
    }
    return [];
  }

  async function requestDetail(nodeId: string): Promise<GraphNodeDetail | null> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      return await connection.invoke('RequestDetail', nodeId);
    }
    return null;
  }

  async function requestHierarchy(query: GraphQuery = {}): Promise<GraphNodeProjection[]> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      return await connection.invoke('RequestHierarchy', query);
    }
    return [];
  }

  async function searchNodes(searchTerm: string): Promise<GraphNodeProjection[]> {
    if (connection?.state === signalR.HubConnectionState.Connected) {
      return await connection.invoke('SearchNodes', searchTerm);
    }
    return [];
  }

  return {
    // State
    isConnected,
    connectionState,
    connectionError,
    
    // Connection methods
    connect,
    disconnect,
    
    // Subscription methods
    subscribeToNode,
    unsubscribeFromNode,
    subscribeToAll,
    
    // Data request methods
    requestChildren,
    requestDetail,
    requestHierarchy,
    searchNodes,
    
    // Event handlers
    onNodeUpdated,
    onNodeDeleted,
    onNodeCreated,
    onStatsUpdated,
    onFilterApplied
  };
}

// Export singleton for global usage
export const globalSignalR = useSignalR();
