import { ComparisonData } from '../types'
import './SummaryCards.css'

interface SummaryCardsProps {
  comparisons: ComparisonData[]
}

export default function SummaryCards({ comparisons }: SummaryCardsProps) {
  const calculateAverage = (key: keyof ComparisonData['improvement']) => {
    if (comparisons.length === 0) return 0
    const sum = comparisons.reduce((acc, comp) => acc + comp.improvement[key], 0)
    return sum / comparisons.length
  }

  const avgLatencyImprovement = calculateAverage('avgLatency')
  const avgP99Improvement = calculateAverage('p99')
  const avgThroughputImprovement = calculateAverage('throughput')
  const avgMemoryImprovement = calculateAverage('memory')

  const cards = [
    {
      title: 'Average Latency',
      value: `${avgLatencyImprovement.toFixed(1)}%`,
      subtitle: 'Improvement',
      color: '#4caf50'
    },
    {
      title: 'P99 Latency',
      value: `${avgP99Improvement.toFixed(1)}%`,
      subtitle: 'Improvement',
      color: '#2196f3'
    },
    {
      title: 'Throughput',
      value: `${avgThroughputImprovement.toFixed(1)}%`,
      subtitle: 'Improvement',
      color: '#ff9800'
    },
    {
      title: 'Memory Usage',
      value: `${avgMemoryImprovement.toFixed(1)}%`,
      subtitle: 'Reduction',
      color: '#f44336'
    }
  ]

  return (
    <div className="summary-cards">
      {cards.map((card, index) => (
        <div key={index} className="summary-card" style={{ borderTopColor: card.color }}>
          <h3>{card.title}</h3>
          <div className="card-value" style={{ color: card.color }}>
            {card.value}
          </div>
          <p className="card-subtitle">{card.subtitle}</p>
        </div>
      ))}
    </div>
  )
}

