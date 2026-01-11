import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import { ComparisonData } from '../types'
import './Chart.css'

interface MemoryChartProps {
  comparisons: ComparisonData[]
}

export default function MemoryChart({ comparisons }: MemoryChartProps) {
  const data = comparisons.map(comp => ({
    name: `${comp.jit.operation} (${comp.jit.datasetSize.toLocaleString()})`,
    'JIT (MB)': comp.jit.memoryMb,
    'AOT (MB)': comp.aot.memoryMb
  }))

  return (
    <div className="chart">
      <h2>Memory Usage Comparison</h2>
      <ResponsiveContainer width="100%" height={400}>
        <BarChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" angle={-45} textAnchor="end" height={100} />
          <YAxis label={{ value: 'Memory (MB)', angle: -90, position: 'insideLeft' }} />
          <Tooltip />
          <Legend />
          <Bar dataKey="JIT (MB)" fill="#f44336" />
          <Bar dataKey="AOT (MB)" fill="#4caf50" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}

