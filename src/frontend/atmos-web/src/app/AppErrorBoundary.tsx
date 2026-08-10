import { Component, type ErrorInfo, type ReactNode } from 'react'

interface AppErrorBoundaryProps {
  children: ReactNode
}

interface AppErrorBoundaryState {
  hasError: boolean
}

export class AppErrorBoundary extends Component<
  AppErrorBoundaryProps,
  AppErrorBoundaryState
> {
  public state: AppErrorBoundaryState = { hasError: false }

  public static getDerivedStateFromError(): AppErrorBoundaryState {
    return { hasError: true }
  }

  public componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unhandled UI error', error, info)
  }

  public render() {
    if (!this.state.hasError) {
      return this.props.children
    }

    return (
      <main className="fatal-error" role="alert">
        <div className="fatal-error__panel">
          <span className="fatal-error__code">Atmos</span>
          <h1>Something unexpected happened.</h1>
          <p>The interface could not be rendered safely. Reload the page to start again.</p>
          <button type="button" onClick={() => window.location.reload()}>
            Reload page
          </button>
        </div>
      </main>
    )
  }
}
