// Backend'in ApiResponse hata formatını (message + errors[]) gösterir.
export default function Errors({ error }) {
  if (!error) return null;
  return (
    <div className="error" role="alert">
      <strong>{error.message}</strong>
      {error.errors?.length > 0 && (
        <ul>{error.errors.map((e, i) => <li key={i}>{e.field ? `${e.field}: ` : ''}{e.message}</li>)}</ul>
      )}
    </div>
  );
}
