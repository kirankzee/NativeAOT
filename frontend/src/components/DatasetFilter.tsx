import './Filter.css'

interface DatasetFilterProps {
  datasets: number[]
  selected: number
  onChange: (dataset: number) => void
}

export default function DatasetFilter({ datasets, selected, onChange }: DatasetFilterProps) {
  return (
    <div className="filter">
      <label htmlFor="dataset-filter">Dataset Size:</label>
      <select
        id="dataset-filter"
        value={selected}
        onChange={(e) => onChange(Number(e.target.value))}
      >
        <option value={0}>All Datasets</option>
        {datasets.map(ds => (
          <option key={ds} value={ds}>{ds.toLocaleString()}</option>
        ))}
      </select>
    </div>
  )
}

