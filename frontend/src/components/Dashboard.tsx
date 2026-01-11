import { ComparisonData } from '../types'
import LatencyChart from './LatencyChart'
import ThroughputChart from './ThroughputChart'
import MemoryChart from './MemoryChart'
import './Dashboard.css'

interface DashboardProps {
  comparisons: ComparisonData[]
}

export default function Dashboard({ comparisons }: DashboardProps) {
  return (
    <div className="dashboard">
      <div className="chart-container">
        <LatencyChart comparisons={comparisons} />
      </div>
      <div className="chart-container">
        <ThroughputChart comparisons={comparisons} />
      </div>
      <div className="chart-container">
        <MemoryChart comparisons={comparisons} />
      </div>
    </div>
  )
}

