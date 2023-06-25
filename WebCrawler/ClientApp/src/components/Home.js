import React, { Component } from "react";
import Container from "react-bootstrap/Container";
import Button from "react-bootstrap/Button";
import { CreateWebsiteRecordModalWindow } from "./CreateWebsiteRecordModalWindow";
import PaginatedView from "./PaginatedView";

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = {
        isCreateWebsiteRecordModalShown: false,
        modalWindowContext: {
            id: 0,
            name: "",
            isActive: true,
            tags: [],
            periodicity: 10,
            regexPattern: ".*",
            entryUrl: "",
            isEditing: false
        }
    };
  }

    onCreateWebsiteRecordModalClose = () => {
        this.setState({ isCreateWebsiteRecordModalShown: false });
        window.location.reload();
    }

    handleEditWebsiteRecord = (context) => {
        this.setState({
            isCreateWebsiteRecordModalShown: true,
            modalWindowContext: {
                ...context,
                tags: [...context.tags],
            }
        });
    }


  render() {
    return (
      <>
        <Container fluid="md" style={{ display: 'flex', flexDirection: 'column'}}>
            <div style={{ flex: '1', marginBottom: '1rem' }}>
                    <PaginatedView showEditModalWindow={ this.handleEditWebsiteRecord } />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                    variant="primary"
                    onClick={() =>
                        this.setState({ isCreateWebsiteRecordModalShown: true })
                    }
                >
                    Create new website record
                </Button>
            </div>
        </Container>

        <CreateWebsiteRecordModalWindow
            show={this.state.isCreateWebsiteRecordModalShown}
            urlPresetValue={this.state.modalWindowContext.entryUrl}
            labelPresetValue={this.state.modalWindowContext.name}
            isActivePresetValue={this.state.modalWindowContext.isActive}
            tagsPresetValue={[...this.state.modalWindowContext.tags]}
            periodicityPresetValue={this.state.modalWindowContext.periodicity}
            regexPresetValue={this.state.modalWindowContext.regexPattern}
            recordId={this.state.modalWindowContext.id}
            isEditing={this.state.modalWindowContext.isEditing}
            passCreatedRecordId={(id) => { }}
            onClose={this.onCreateWebsiteRecordModalClose}
        />
      </>
    );
  }
}
