const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export interface ShipInfo {
  shipId: string;
  name: string;
  imoNumber?: string;
  status: string;
  lastSeen?: string;
}

export interface EventSummary {
  shipId: string;
  totalEvents: number;
  criticalEvents: number;
  operationalEvents: number;
  telemetryEvents: number;
  lastEventAt?: string;
}

export interface StoredEvent {
  id: string;
  shipId: string;
  eventType: string;
  priority: string;
  timestamp: string;
  jsonPayload: string;
}

export async function getFleet(): Promise<ShipInfo[]> {
  const res = await fetch(`${API_BASE}/api/fleet`);
  if (!res.ok) throw new Error('Failed to fetch fleet');
  return res.json();
}

export async function getShipEvents(shipId: string, limit = 50): Promise<StoredEvent[]> {
  const res = await fetch(`${API_BASE}/api/events/${shipId}?limit=${limit}`);
  if (!res.ok) throw new Error('Failed to fetch events');
  return res.json();
}

export async function getEventSummary(shipId: string): Promise<EventSummary> {
  const res = await fetch(`${API_BASE}/api/events/${shipId}/summary`);
  if (!res.ok) throw new Error('Failed to fetch summary');
  return res.json();
}

export async function getCriticalEvents(limit = 20): Promise<StoredEvent[]> {
  const res = await fetch(`${API_BASE}/api/events/critical?limit=${limit}`);
  if (!res.ok) throw new Error('Failed to fetch critical events');
  return res.json();
}
