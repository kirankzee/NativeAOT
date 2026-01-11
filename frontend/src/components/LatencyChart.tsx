import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { ComparisonData } from '../types'
import './Chart.css'

interface LatencyChartProps {
  comparisons: ComparisonData[]
}

export default function LatencyChart({ comparisons }: LatencyChartProps) {
  const data = comparisons.map(comp => ({
    name: `${comp.jit.operation} (${comp.jit.datasetSize.toLocaleString()})`,
    'JIT Avg (ms)': comp.jit.avgLatencyMs,
    'AOT Avg (ms)': comp.aot.avgLatencyMs,
    'JIT P99 (ms)': comp.jit.p99,
    'AOT P99 (ms)': comp.aot.p99
  }))

  return (
    <div className="chart">
      <h2>Latency Comparison</h2>
      <ResponsiveContainer width="100%" height={400}>
        <BarChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
          <YAxis label={{ value: 'Latency (ms)', angle: -90, position: 'insideLeft' }} />
          <Tooltip />
          <Legend />
          <Bar dataKey="JIT Avg (ms)" fill="#f44336" />
          <Bar dataKey="AOT Avg (ms)" fill="#4caf50" />
          <Bar dataKey="JIT P99 (ms)" fill="#ff9800" />
          <Bar dataKey="AOT P99 (ms)" fill="#2196f3" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}

