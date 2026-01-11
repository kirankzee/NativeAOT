import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { ComparisonData } from '../types'
import './Chart.css'

interface ThroughputChartProps {
  comparisons: ComparisonData[]
}

export default function ThroughputChart({ comparisons }: ThroughputChartProps) {
  const data = comparisons.map(comp => ({
    name: `${comp.jit.operation} (${comp.jit.datasetSize.toLocaleString()})`,
    'JIT (RPS)': comp.jit.throughputRps,
    'AOT (RPS)': comp.aot.throughputRps
  }))

  return (
    <div className="chart">
      <h2>Throughput Comparison (Requests Per Second)</h2>
      <ResponsiveContainer width="100%" height={400}>
        <BarChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
          <YAxis label={{ value: 'RPS', angle: -90, position: 'insideLeft' }} />
          <Tooltip />
          <Legend />
          <Bar dataKey="JIT (RPS)" fill="#f44336" />
          <Bar dataKey="AOT (RPS)" fill="#4caf50" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}

