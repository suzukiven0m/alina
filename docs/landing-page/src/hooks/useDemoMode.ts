import { useState, useEffect } from 'react';
import type { ShipInfo, EventSummary, StoredEvent } from '../api/fleetApi';

export function useDemoMode() {
  const [isDemo, setIsDemo] = useState(false);

  useEffect(() => {
    fetch(
      `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/health`,
      { signal: AbortSignal.timeout(2000) }
    ).catch(() => setIsDemo(true));
  }, []);

  return isDemo;
}

export const demoShips: ShipInfo[] = [
  { shipId: 'MSC-001', name: 'MSC Alina', imoNumber: '9074729', status: 'Online', lastSeen: new Date().toISOString(), latitude: 36.15, longitude: -5.35, speed: 18.5 },
  { shipId: 'MSC-002', name: 'MSC Bravo', imoNumber: '9074731', status: 'Online', lastSeen: new Date().toISOString(), latitude: 40.45, longitude: -3.72, speed: 21.0 },
  { shipId: 'MSC-003', name: 'MSC Charlie', imoNumber: '9074743', status: 'Offline', lastSeen: new Date(Date.now() - 3600000).toISOString(), latitude: 51.9, longitude: 4.5, speed: 0 },
];

export const demoSummaries: Record<string, EventSummary> = {
  'MSC-001': { shipId: 'MSC-001', totalEvents: 1247, criticalEvents: 3, operationalEvents: 42, telemetryEvents: 1202, lastEventAt: new Date().toISOString() },
  'MSC-002': { shipId: 'MSC-002', totalEvents: 892, criticalEvents: 0, operationalEvents: 18, telemetryEvents: 874, lastEventAt: new Date().toISOString() },
  'MSC-003': { shipId: 'MSC-003', totalEvents: 3456, criticalEvents: 1, operationalEvents: 67, telemetryEvents: 3388, lastEventAt: new Date(Date.now() - 3600000).toISOString() },
};

export const demoCriticalEvents: StoredEvent[] = [
  {
    id: 'e1',
    shipId: 'MSC-001',
    eventType: 'alert.triggered',
    priority: 'Critical',
    timestamp: new Date(Date.now() - 120000).toISOString(),
    jsonPayload: '{"ruleName":"Engine Overheat","message":"Engine temp 115.2°C exceeds threshold"}',
  },
  {
    id: 'e2',
    shipId: 'MSC-003',
    eventType: 'alert.triggered',
    priority: 'Critical',
    timestamp: new Date(Date.now() - 600000).toISOString(),
    jsonPayload: '{"ruleName":"Bilge High Water","message":"Bilge level at 82%"}',
  },
];
